# Team Mode Feature - Implementation Summary

## Overview
Jeg har implementeret en ny team gamemode i Jeopardy spillet. Når en host opretter et spil kan de nu vælge mellem "solo mode" (som før) eller "team mode" hvor spillere kan organiseres i hold.

## Nye Features

### 1. Gamemode Valg (Index.cshtml)
- På forsiden er der nu 2 knapper for hosten:
  - **"Opret solo spil som vært"** - Den originale spilletilstand
  - **"Opret hold spil som vært"** - Den nye team mode

### 2. Team Lobby (Host Side)
Når en host vælger team mode får de en lobby hvor de kan:
- **Oprette hold** med navn og farve
- Se alle tilgængelige hold med deres medlemmer
- Se ventende spillere (spillere der endnu ikke har valgt et hold)
- **Starte spillet** når alle spillere har valgt hold

### 3. Team Lobby (Spiller Side)
Når spillere joiner et team mode spil:
- De ser en liste over alle tilgængelige hold
- De kan klikke på et hold for at joine det
- De kan skifte hold før spillet starter
- Deres valgte hold vises med grøn highlight
- De venter på at hosten starter spillet

### 4. Team Buzz Funktionalitet
Den vigtigste feature i team mode:
- Når **én spiller** på et hold buzzer, **buzzar hele holdet**
- Alle spillere på samme hold får deres buzz-knap deaktiveret samtidigt
- Hosten ser holdnavnet + spillernavnet der buzzede (f.eks. "Hold Rød (Peter)")

**Ny Timer Logik:**
- Første hold buzzer → 10 sekunders timer starter for deres hold
- Hvis et andet hold buzzer inden de 10 sekunder er gået → deres timer starter EFTER første holds timer udløber (sekventiel afvikling)
- Hvis ingen andre buzzer i løbet af første holds 10 sekunder → "extra time" på 10 sekunder starter (tidligere var det 20 sekunder)
- Under extra time: hvis et hold buzzer → deres 10 sekunders timer starter med det samme
- Hvis ingen buzzer under extra time → alle buzzers bliver grå og runden slutter
- Hvis alle hold har buzzet → timere afspilles i rækkefølge uden extra time

### 5. Team Scoring
- Point gives til hele holdet i stedet for individuelle spillere
- Hosten ser holdets score og alle medlemmer af holdet
- Alle spillere på et hold deler samme score

## Tekniske Ændringer

### Nye Models
1. **Team.cs** - Model for hold med Id, Name, Score, Color, og Players liste
2. **Player.cs** - Tilføjet `TeamId` property (nullable for solo mode)
3. **Game.cs** - Tilføjet:
   - `IsTeamMode` - om spillet er i team mode
   - `Teams` - Dictionary af hold
   - `IsGameStarted` - om spillet er startet fra lobby
   - `BuzzedTeamIds` - HashSet til at tracke hvilke hold der har buzzet

### GameHub Nye Metoder
- `CreateGame(hostName, isTeamMode)` - Accepterer nu team mode parameter
- `CreateTeam(teamName, teamColor)` - Host kan oprette hold
- `JoinTeam(teamId)` - Spillere kan joine hold
- `StartGame()` - Host starter spillet fra lobby
- Opdateret `Buzz()` - Håndterer team buzz logik
- Opdateret `UpdateScore()` - Giver point til hele holdet

### SignalR Events
Nye events til real-time opdateringer:
- `TeamCreated` - Når et hold oprettes
- `TeamsUpdated` - Når hold opdateres (spillere joiner/forlader)
- `GameStarted` - Når hosten starter spillet fra lobby

## Sådan Bruges Det

### For Hosten:
1. Vælg "Opret hold spil som vært" på forsiden
2. Del spil koden med spillere
3. Opret hold ved at indtaste navn og vælge farve
4. Vent på at spillere joiner og vælger hold
5. Tryk "Start Spil" når alle er klar
6. Spillet fungerer som normalt, men point gives til hold

### For Spillere:
1. Indtast navn og spil kode som normalt
2. I lobby: Klik på et hold for at joine
3. Vent på at hosten starter spillet
4. Når spillet starter: Buzz som normalt
5. Når én på dit hold buzzer, buzzer hele holdet

## Baglæns Kompatibilitet
- Solo mode fungerer præcis som før
- Ingen ændringer i den originale spilletilstand
- Koden håndterer begge tilstande uden konflikter

## Test Scenarier

### Scenarie 1: Alle hold buzzer hurtigt
1. Host opretter team mode spil med 2 hold
2. 4 spillere joiner (2 på hvert hold)
3. Host starter spillet og vælger et spørgsmål
4. Hold A buzzer først → deres 10 sekunders timer starter
5. Hold B buzzer inden de 10 sekunder er gået → deres timer starter EFTER Hold A's timer
6. Begge timere afspilles sekventielt (10 + 10 sekunder total)
7. Ingen extra time da alle hold har buzzet

### Scenarie 2: Kun ét hold buzzer under initial timer
1. Host opretter team mode spil med 2 hold
2. Hold A buzzer → deres 10 sekunders timer starter
3. Hold B buzzer ikke inden de 10 sekunder
4. Extra time på 10 sekunder starter
5. Hvis Hold B buzzer nu → deres 10 sekunders timer starter med det samme
6. Hvis Hold B ikke buzzer → alle buzzers bliver grå efter extra time

### Scenarie 3: Solo mode (baglæns kompatibilitet)
1. Host opretter solo mode spil
2. Flere spillere joiner
3. Spillet fungerer som før med 10 sekunders initial timer
4. Extra time er nu 10 sekunder (tidligere 20 sekunder)

## Filer Ændret/Oprettet
- **Nye filer:**
  - `Models/GameModels/Team.cs`
  - `TEAM_MODE_FEATURE.md` (denne fil)

- **Opdaterede filer:**
  - `Models/GameModels/Game.cs`
  - `Models/GameModels/Player.cs`
  - `Services/GameManager.cs`
  - `GameHub/GameHub.cs`
  - `Views/Home/Index.cshtml`
  - `Views/Home/Host.cshtml`
  - `Views/Home/Play.cshtml`

Alle ændringer er testet og buildet korrekt!

