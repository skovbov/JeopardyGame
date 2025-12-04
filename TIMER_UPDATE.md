# Timer Update - Implementation Summary

## Ændringer Implementeret

### 1. Extra Time Reduceret
- **Før:** 20 sekunders extra time
- **Nu:** 10 sekunders extra time
- Gælder både solo mode og team mode

### 2. Ny Sekventiel Timer Logik for Team Mode

#### Hvordan det virker nu:

**Første hold buzzer:**
- 10 sekunders timer starter for deres hold
- Hosten ser progress bar med "Første buzz: X"
- Spillere ser "Dit holds tid: 10s" eller "Andet hold svarer..."

**Hvis andet hold buzzer inden 10 sekunder:**
- Deres buzz registreres
- INGEN timer starter endnu for dem
- De venter til første holds timer udløber
- Derefter starter deres 10 sekunders timer automatisk

**Hvis INGEN andre buzzer inden 10 sekunder:**
- Extra time på 10 sekunder starter
- Alle hold der ikke har buzzet kan stadig buzze
- Progress bar skifter til "Extra tid: X"

**Under extra time:**
- Hvis et hold buzzer → deres 10 sekunders timer starter med det samme
- Hvis ingen buzzer → alle buzzers bliver grå og runden slutter

**Hvis alle hold har buzzet:**
- Alle timere afspilles sekventielt (10 + 10 + 10... sekunder)
- INGEN extra time da alle hold allerede har buzzet

### 3. Tekniske Ændringer

#### Game Model (Game.cs)
Tilføjet nye properties:
```csharp
public bool IsTimerRunning { get; set; } = false;
public bool IsExtraTimeActive { get; set; } = false;
```

#### GameHub.cs
- **Ny metode:** `ProcessTeamTimers(Game game)` - Håndterer den sekventielle timer logik
- **Opdateret:** `Buzz()` - Tjekker timer state og starter timer korrekt
- **Opdateret:** `ResetBuzz()` og `SelectQuestion()` - Nulstiller timer state

#### SignalR Events
- **Nyt event:** `BuzzExtraTimeStarted` - Sendes når extra time starter (10 sekunder)
- **Opdateret:** `BuzzTimerStarted` - Sender nu teamId og duration for team mode

#### Host.cshtml
- Opdateret `startExtendedBuzzTimer()` til 10 sekunder
- Tilføjet event handler for `BuzzExtraTimeStarted`
- Progress bar viser nu korrekt "Extra tid: 10" i stedet for "Extra tid: 20"

#### Play.cshtml
- Opdateret `BuzzTimerStarted` handler til at vise teamspecifik information
- Tilføjet `BuzzExtraTimeStarted` handler til at vise extra time besked
- Spillere ser nu "Dit holds tid" vs "Andet hold svarer"

## Timer Flow Eksempler

### Eksempel 1: To hold, begge buzzer hurtigt
```
0s:  Hold A buzzer → Timer starter for Hold A
3s:  Hold B buzzer → Buzz registreres, venter på Hold A's timer
10s: Hold A's timer udløber → Hold B's timer starter automatisk
20s: Hold B's timer udløber → Færdig (ingen extra time)
```

### Eksempel 2: To hold, kun et buzzer først
```
0s:  Hold A buzzer → Timer starter for Hold A
10s: Hold A's timer udløber → Extra time starter (10 sekunder)
15s: Hold B buzzer under extra time → Hold B's timer starter med det samme
25s: Hold B's timer udløber → Færdig
```

### Eksempel 3: To hold, ingen buzzer under extra time
```
0s:  Hold A buzzer → Timer starter for Hold A
10s: Hold A's timer udløber → Extra time starter (10 sekunder)
20s: Extra time udløber → Alle buzzers bliver grå, runden slutter
```

### Eksempel 4: Tre hold, alle buzzer
```
0s:  Hold A buzzer → Timer starter for Hold A
5s:  Hold B buzzer → Venter
8s:  Hold C buzzer → Venter
10s: Hold A's timer udløber → Hold B's timer starter
20s: Hold B's timer udløber → Hold C's timer starter
30s: Hold C's timer udløber → Færdig (ingen extra time)
```

## Baglæns Kompatibilitet
- Solo mode fungerer stadig som før
- Eneste ændring i solo mode: Extra time er nu 10 sekunder i stedet for 20
- Ingen breaking changes i eksisterende funktionalitet

## Test Checklist
- [x] Build succeeds uden fejl
- [x] Extra time er 10 sekunder i både solo og team mode
- [x] Sekventiel timer logik fungerer i team mode
- [x] Progress bar viser korrekte værdier
- [x] Event handlers er tilføjet og fungerer
- [x] Dokumentation opdateret

## Filer Ændret
1. `Models/GameModels/Game.cs` - Tilføjet timer state properties
2. `GameHub/GameHub.cs` - Implementeret ny timer logik
3. `Views/Home/Host.cshtml` - Opdateret timer display og handlers
4. `Views/Home/Play.cshtml` - Opdateret spiller UI og handlers
5. `TEAM_MODE_FEATURE.md` - Opdateret dokumentation
6. `TIMER_UPDATE.md` - Denne fil

Alle ændringer er implementeret og testet!

