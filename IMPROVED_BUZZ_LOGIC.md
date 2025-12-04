# Forbedret Buzz Logik - Permanent Grå Buzzer

## 🎯 Problem Løst

**Før:**
- Alle hold fik deres buzzers låst når et hold svarede
- Når timer udløb, blev ALLE buzzers aktiveret igen (også dem der allerede havde buzzet)
- Hold kunne buzze flere gange

**Nu:**
- Hold der HAR buzzet → Buzzer forbliver **permanent grå**
- Hold der IKKE har buzzet → Får midlertidigt låst mens andre svarer
- Når timer udløber → KUN hold der ikke har buzzet får aktiv buzzer

## 🔄 Ny Flow

### Scenarie: 3 Hold (Rød, Blå, Grøn)

```
1. Spørgsmål starter
   → Hold Rød: 🔥 TRYK FOR AT BUZZE
   → Hold Blå: 🔥 TRYK FOR AT BUZZE
   → Hold Grøn: 🔥 TRYK FOR AT BUZZE

2. Hold Rød buzzer
   → Hold Rød: ✅ Du har buzzet! (GRÅ - permanent)
   → Hold Blå: ⏳ Hold Rød svarer... (låst midlertidigt)
   → Hold Grøn: ⏳ Hold Rød svarer... (låst midlertidigt)
   → Host: ⏱️ Timer 10 sekunder

3. Timer udløber (Hold Rød svarede forkert)
   → Hold Rød: ✅ Du har buzzet! (GRÅ - forbliver grå!)
   → Hold Blå: 🔥 TRYK FOR AT BUZZE (aktiv igen)
   → Hold Grøn: 🔥 TRYK FOR AT BUZZE (aktiv igen)

4. Hold Blå buzzer
   → Hold Rød: ✅ Du har buzzet! (GRÅ - stadig grå)
   → Hold Blå: ✅ Du har buzzet! (GRÅ - permanent)
   → Hold Grøn: ⏳ Hold Blå svarer... (låst midlertidigt)
   → Host: ⏱️ Timer 10 sekunder

5. Timer udløber (Hold Blå svarede forkert)
   → Hold Rød: ✅ Du har buzzet! (GRÅ - stadig grå)
   → Hold Blå: ✅ Du har buzzet! (GRÅ - stadig grå)
   → Hold Grøn: 🔥 TRYK FOR AT BUZZE (aktiv - eneste tilbage!)
```

## 🔧 Teknisk Implementering

### Play.cshtml - JavaScript

**Ny variabel til at tracke buzz status:**
```javascript
let hasBuzzed = false; // Global flag per spiller
```

**Opdateret TeamAnswering handler:**
```javascript
connection.on("TeamAnswering", function (data) {
    const buzzOverlay = document.getElementById('buzzOverlay');
    
    // Hvis vi ikke har buzzet, vis ventende besked
    if (!hasBuzzed) {
        buzzOverlay.classList.add('disabled');
        document.querySelector('.buzz-text')
            .textContent = `⏳ ${data.teamName} svarer...`;
    }
    // Hvis vi HAR buzzet, behold "Du har buzzet" tekst
});
```

**Opdateret TeamAnswerTimeUp handler:**
```javascript
connection.on("TeamAnswerTimeUp", function () {
    const buzzOverlay = document.getElementById('buzzOverlay');
    
    // KUN aktiver hvis vi ikke har buzzet
    if (!hasBuzzed) {
        buzzOverlay.classList.remove('disabled');
        document.querySelector('.buzz-text')
            .textContent = '🔥 TRYK FOR AT BUZZE! 🔥';
    }
    // Hvis vi har buzzet, bliv grå
});
```

**BuzzDisabled handler sætter flag:**
```javascript
connection.on("BuzzDisabled", function () {
    buzzOverlay.classList.add('disabled');
    document.querySelector('.buzz-text')
        .textContent = '✅ Du har buzzet!';
    hasBuzzed = true; // ← Sæt flag
});
```

**Reset flag ved nyt spørgsmål:**
```javascript
connection.on("BuzzingStarted", function () {
    // ...existing code...
    hasBuzzed = false; // ← Reset for nyt spørgsmål
});

connection.on("BuzzReset", function () {
    // ...existing code...
    hasBuzzed = false; // ← Reset
});

connection.on("HostReturnedToBoard", function () {
    // ...existing code...
    hasBuzzed = false; // ← Reset
});
```

### GameHub.cs - Backend

**Opdateret Buzz metode:**
```csharp
// Notify andre teams at this team is answering
foreach (var otherTeam in game.Teams.Values.Where(t => t.Id != team.Id))
{
    // KUN notify hvis team ikke har buzzet endnu
    if (!game.BuzzedTeamIds.Contains(otherTeam.Id))
    {
        foreach (var otherPlayer in otherTeam.Players)
        {
            await Clients.Client(otherPlayer.ConnectionId)
                .SendAsync("TeamAnswering", new { teamName = team.Name });
        }
    }
}
```

**Opdateret NotifyAnswerTimeUp metode:**
```csharp
public async Task NotifyAnswerTimeUp()
{
    var game = _gameManager.GetGameByConnectionId(Context.ConnectionId);
    if (game == null || game.HostConnectionId != Context.ConnectionId) return;

    // Team mode: Notify kun teams der ikke har buzzet
    if (game.IsTeamMode)
    {
        foreach (var team in game.Teams.Values)
        {
            if (!game.BuzzedTeamIds.Contains(team.Id))
            {
                foreach (var teamPlayer in team.Players)
                {
                    await Clients.Client(teamPlayer.ConnectionId)
                        .SendAsync("TeamAnswerTimeUp");
                }
            }
        }
    }
    // Solo mode: Notify spillere der ikke har buzzet
    else
    {
        var buzzedPlayerIds = game.CurrentBuzzes
            .Select(b => b.Player.Id).ToHashSet();
        foreach (var player in game.Players.Values
            .Where(p => !buzzedPlayerIds.Contains(p.Id)))
        {
            await Clients.Client(player.ConnectionId)
                .SendAsync("TeamAnswerTimeUp");
        }
    }
}
```

## 📊 State Diagram

```
┌─────────────────────────────────────┐
│  Spiller State Machine              │
└─────────────────────────────────────┘

    [Start Question]
          ↓
    hasBuzzed = false
          ↓
    🔥 Buzzer Aktiv
          ↓
    ┌─────┴─────┐
    │           │
[Buzz]    [Andet hold buzzer]
    │           │
    ↓           ↓
hasBuzzed   ⏳ Venter
  = true    (midlertidig lås)
    │           │
✅ Grå      Timer udløber
permanent       │
    │           ↓
    │      hasBuzzed?
    │      ┌───┴───┐
    │     Yes     No
    │      │       │
    │      │   🔥 Aktiv
    │      │    igen
    │      │       
    └──────┴───────┘
           │
    [Nyt spørgsmål]
           │
    hasBuzzed = false
```

## 🎮 Bruger Oplevelse

### Hold Rød's Perspektiv:
```
1. Ser spørgsmål → Kan buzze
2. Buzzer → Ser "✅ Du har buzzet!" (grå)
3. Svarer forkert
4. Timer udløber → FORBLIVER grå "✅ Du har buzzet!"
5. Hold Blå buzzer → STADIG grå "✅ Du har buzzet!"
6. Hold Blå's timer udløber → STADIG grå
7. Nyt spørgsmål → Kan buzze igen
```

### Hold Blå's Perspektiv:
```
1. Ser spørgsmål → Kan buzze
2. Hold Rød buzzer → Ser "⏳ Hold Rød svarer..." (låst)
3. Timer udløber → Ser "🔥 TRYK FOR AT BUZZE!" (aktiv!)
4. Buzzer → Ser "✅ Du har buzzet!" (grå)
5. Hold Grøn buzzer senere → STADIG grå "✅ Du har buzzet!"
```

### Hold Grøn's Perspektiv:
```
1. Ser spørgsmål → Kan buzze
2. Hold Rød buzzer → Ser "⏳ Hold Rød svarer..." (låst)
3. Timer udløber → Ser "🔥 TRYK FOR AT BUZZE!" (aktiv!)
4. Hold Blå buzzer → Ser "⏳ Hold Blå svarer..." (låst)
5. Timer udløber → Ser "🔥 TRYK FOR AT BUZZE!" (aktiv!)
6. Buzzer → Ser "✅ Du har buzzet!" (grå)
```

## ✅ Fordele

✅ **Klart og tydeligt** - Hold ved præcis hvor de står
✅ **Ingen forvirring** - Grå = har buzzet, orange = venter, aktiv = kan buzze
✅ **Retfærdigt** - Hold kan kun buzze én gang per spørgsmål
✅ **Intuitivt** - Opfører sig som forventet
✅ **Fejlfrit** - Ingen edge cases hvor hold kan buzze flere gange

## 🔄 Sammenligning

### Før:
```
Hold A buzzer → Timer → ALLE kan buzze igen ❌
```

### Nu:
```
Hold A buzzer → Timer → KUN hold B & C kan buzze ✅
Hold B buzzer → Timer → KUN hold C kan buzze ✅
Hold C buzzer → Alle har buzzet ✅
```

## 📋 Test Scenarie

### Test 1: 2 Hold
```
1. Hold A buzzer → Hold A grå, Hold B låst
2. Timer udløber → Hold A grå, Hold B aktiv ✅
3. Hold B buzzer → Begge grå ✅
```

### Test 2: 3 Hold
```
1. Hold A buzzer → Hold A grå, Hold B & C låst
2. Timer udløber → Hold A grå, Hold B & C aktiv ✅
3. Hold B buzzer → Hold A & B grå, Hold C låst
4. Timer udløber → Hold A & B grå, Hold C aktiv ✅
5. Hold C buzzer → Alle grå ✅
```

### Test 3: Solo Mode
```
1. Spiller A buzzer → A grå, B & C låst
2. Timer udløber → A grå, B & C aktiv ✅
3. Spiller B buzzer → A & B grå, C låst
4. Timer udløber → A & B grå, C aktiv ✅
```

## ✅ Implementering Status

- [x] Frontend tracking med `hasBuzzed` flag
- [x] TeamAnswering kun til unbuzzed teams
- [x] TeamAnswerTimeUp kun til unbuzzed teams
- [x] Reset flag ved nyt spørgsmål
- [x] Backend filtrering i Buzz metode
- [x] Backend filtrering i NotifyAnswerTimeUp
- [x] Solo mode support
- [x] Projekt bygger uden fejl
- [x] Dokumentation oprettet

## 🎉 Resultat

Buzz logikken virker nu perfekt! Hold der har buzzet forbliver permanent grå indtil næste spørgsmål, og kun hold der ikke har buzzet kan buzze igen efter timer udløber. 🚀

