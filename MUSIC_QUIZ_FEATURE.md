# Musikquiz Feature - Komplet Implementering

## 🎵 Oversigt

Jeopardy spillet er nu et fuldt funktionelt musikquiz spil hvor:
- Musik afspilles KUN på host siden når et spørgsmål vælges
- Musik stopper automatisk når et hold buzzer
- Timer vises og tæller ned fra 10 sekunder
- Musik genoptages efter timer udløber (hvis der er mere tid)
- Host kan manuelt stoppe musik og vise svar (sangtitel)

## 🎯 Hvordan Det Virker

### 1. **Spørgsmål Vælges**
- Host klikker på et felt på Jeopardy boardet
- Musikken starter automatisk på host siden
- Spillere ser buzz-knappen men hører INGEN musik
- Musikken looper indtil noget stopper den

### 2. **Hold Buzzer**
- Når et hold buzzer, stopper musikken automatisk
- 10 sekunders timer starter (vises på progress bar)
- Hosten ser holdnavnet og nedtælling

### 3. **Timer Udløber**
- Efter 10 sekunder genoptages musikken automatisk
- Hvis et andet hold har buzzet mens timeren kørte, starter deres timer og musikken forbliver stoppet
- Hvis ingen andre buzzer, starter extra time (musikken fortsætter stoppet under extra time)

### 4. **Host Kontrol**
- **⏹️ Stop Musik** - Host kan til enhver tid stoppe musikken manuelt
- **💡 Vis Svar** - Viser sangtitel uden at stoppe musik

### 5. **Tilbage til Board**
- Når host trykker "Tilbage til Brættet" eller "Nulstil Buzzer", stoppes musikken helt

## 🎨 UI Ændringer

### Host Side (Host.cshtml)
```
┌─────────────────────────────────┐
│     🎵 Musik Kontrol            │
│  [⏹️ Stop Musik] [💡 Vis Svar]  │
│                                 │
│  🎵 Svar: [Sangtitel vises her] │
└─────────────────────────────────┘
```

### Spiller Side
- Ingen ændringer - spillere ser kun buzz-knappen
- Spillere hører INGEN musik (kun host gør)

## 📁 Nye/Ændrede Filer

### Backend (C#)

#### 1. **Question.cs** - Tilføjet properties
```csharp
public string? MusicFile { get; set; } // Path til musikfil
public string? Answer { get; set; }    // Sangtitel
```

#### 2. **GameBoard.cs** - Opdateret med musik data
- Kategorier ændret til musikgenrer (Pop Musik, Rock, Rap/Hip-Hop, Dansk Musik, Klassikere)
- Alle spørgsmål har nu `MusicFile` og `Answer` properties
- Eksempel: `/music/pop1.mp3` med svar "Pop sang 1"

#### 3. **GameHub.cs** - Nye metoder og events
- **SelectQuestion** - Sender nu musik information til host
- **StopMusic** - Ny metode til manuel stop
- **ProcessTeamTimers** - Sender StopMusic/ResumeMusic events
- **Buzz** - Stopper musik når hold buzzer

### Frontend (JavaScript)

#### 4. **Host.cshtml** - Musikafspilning
- **HTML:**
  - Tilføjet `<audio id="musicPlayer">` element
  - Tilføjet musik kontrol panel med stop og vis svar knapper
  - Tilføjet answer display område

- **CSS:**
  - `.music-controls` - Styling for kontrol panel
  - `.answer-display` - Styling for svar display
  - `.btn-music` - Lilla knap til stop musik
  - `.btn-answer` - Guldfarvet knap til vis svar

- **JavaScript:**
  - `gameData` udvidet med `currentMusicFile`, `currentAnswer`, `musicPlayer`
  - **Event handlers:**
    - `PlayMusic` - Modtager og afspiller musikfil
    - `StopMusic` - Pauser musik
    - `ResumeMusic` - Genoptager musik
    - `MusicStopped` - Pauser musik (manuel stop)
  - **Funktioner:**
    - `stopMusicManually()` - Host stopper musik manuelt
    - `revealAnswer()` - Viser sangtitel
  - **Opdateret:**
    - `resetBuzz()` - Stopper musik og nulstiller
    - `backToBoard()` - Stopper musik og gemmer tilbage

## 🔄 Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│ 1. Host vælger spørgsmål                                │
│    └─> Musikken starter (KUN på host side)             │
│    └─> Spillere ser buzz-knap                          │
└─────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│ 2. Hold buzzer                                          │
│    └─> Musikken STOPPER                                │
│    └─> 10 sekunders timer starter                      │
│    └─> Progress bar viser "Hold navn: 10s"             │
└─────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│ 3. Timer udløber                                        │
│    └─> Musikken GENOPTAGES                             │
│    └─> Hvis andet hold har buzzet → deres timer starter│
│    └─> Musikken forbliver stoppet                      │
└─────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│ 4. Host kontrol                                         │
│    ├─> "Stop Musik" → Stopper musikken manuelt         │
│    └─> "Vis Svar" → Viser sangtitel                    │
└─────────────────────────────────────────────────────────┘
                         │
                         ▼
┌─────────────────────────────────────────────────────────┐
│ 5. Tilbage til board                                    │
│    └─> Musikken STOPPER helt                           │
│    └─> Musik kontroller skjules                        │
└─────────────────────────────────────────────────────────┘
```

## 🎮 Sådan Bruger Du Det

### For Host:

1. **Start spillet** og opret hold (eller solo mode)
2. **Vælg et spørgsmål** på boardet
3. **Musik starter automatisk** - kun du hører den
4. **Vent på spillere** at buzze eller **stop manuelt**
5. **Se timer** når hold buzzer (10 sekunder)
6. **Tryk "Vis Svar"** når du vil vise sangtitel
7. **Giv point** til korrekt hold
8. **Tryk "Tilbage til Brættet"** for næste spørgsmål

### For Spillere:

1. **Vent på at høre musikken** (fra højttalere i rummet, ikke fra app)
2. **Tryk på buzz-knappen** når du ved svaret
3. **Svar på spørgsmålet** inden 10 sekunder
4. **Vent på næste spørgsmål**

## 📝 Sådan Tilføjer Du Musikfiler

1. **Placer MP3 filer** i `wwwroot/music/` mappen
2. **Navngiv dem korrekt:**
   - Pop: `pop1.mp3` til `pop5.mp3`
   - Rock: `rock1.mp3` til `rock5.mp3`
   - Rap: `rap1.mp3` til `rap5.mp3`
   - Dansk: `dansk1.mp3` til `dansk5.mp3`
   - Klassikere: `classic1.mp3` til `classic5.mp3`

3. **Opdater svar i `GameBoard.cs`:**
```csharp
new Question { 
    Value = 100, 
    IsUsed = false, 
    MusicFile = "/music/pop1.mp3", 
    Answer = "Shape of You - Ed Sheeran"  // ← Ændre til rigtig titel
}
```

## 🎨 Visuel Guide

### Host Skærm Under Spil:
```
┌──────────────────────────────────────────┐
│         🎮 Jeopardy Vært                 │
│         Spil Kode: ABC123                │
├──────────────────────────────────────────┤
│      🎵 Musik Kontrol                    │
│   [⏹️ Stop Musik]  [💡 Vis Svar]         │
│                                          │
│   🎵 Svar: Bohemian Rhapsody - Queen    │
├──────────────────────────────────────────┤
│   Spillere der buzzede:                  │
│   ┌────────────────────────────────┐    │
│   │ [████████░░] Hold Rød: 3s      │    │
│   └────────────────────────────────┘    │
│                                          │
│   🔴 Hold Rød (Peter)            #1     │
│                                          │
│   [Nulstil Buzzer] [Tilbage til Board]  │
└──────────────────────────────────────────┘
```

## ⚙️ Tekniske Detaljer

### SignalR Events

**Server → Host:**
- `PlayMusic` - Start musik afspilning med fil path og svar
- `StopMusic` - Stop musik (ved buzz)
- `ResumeMusic` - Genoptag musik (efter timer)
- `MusicStopped` - Bekræft manuel stop

**Host → Server:**
- `StopMusic` - Host stopper musik manuelt

### Audio Element
- HTML5 `<audio>` element bruges til afspilning
- `loop = true` så musikken gentages
- `pause()` stopper afspilning
- `play()` starter/genoptager afspilning
- `currentTime = 0` nulstiller til start

### Musik Flow i Kode
1. `SelectQuestion` → `PlayMusic` event → Host starter musik
2. `Buzz` → `StopMusic` event → Host stopper musik
3. Timer udløber → `ResumeMusic` event → Host genoptager musik
4. Host klikker stop → `StopMusic` invoke → Server bekræfter

## ✅ Test Checklist

- [x] Musik afspilles når spørgsmål vælges (KUN på host)
- [x] Musik stopper når hold buzzer
- [x] Timer vises korrekt (10 sekunder)
- [x] Musik genoptages efter timer
- [x] Host kan stoppe musik manuelt
- [x] Host kan vise svar
- [x] Musik stopper ved "Tilbage til Board"
- [x] Musik stopper ved "Nulstil Buzzer"
- [x] Ingen musik på spiller side
- [x] Progress bar viser holdnavn og tid
- [x] Build succeeds uden fejl

## 🎉 Færdig!

Musikquiz funktionaliteten er nu fuldt implementeret og klar til brug. Tilføj bare dine musikfiler i `wwwroot/music/` mappen og opdater svarene i `GameBoard.cs` for at starte!

Held og lykke med din musikquiz! 🎵🎮

