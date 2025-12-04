# Musik Filer til Jeopardy Musikquiz

## Sådan Tilføjer Du Musikfiler

1. **Placer dine MP3 filer i denne mappe** (`wwwroot/music/`)

2. **Navngiv filerne som i GameBoard.cs:**
   - Pop: `pop1.mp3`, `pop2.mp3`, `pop3.mp3`, `pop4.mp3`, `pop5.mp3`
   - Rock: `rock1.mp3`, `rock2.mp3`, `rock3.mp3`, `rock4.mp3`, `rock5.mp3`
   - Rap/Hip-Hop: `rap1.mp3`, `rap2.mp3`, `rap3.mp3`, `rap4.mp3`, `rap5.mp3`
   - Dansk: `dansk1.mp3`, `dansk2.mp3`, `dansk3.mp3`, `dansk4.mp3`, `dansk5.mp3`
   - Klassikere: `classic1.mp3`, `classic2.mp3`, `classic3.mp3`, `classic4.mp3`, `classic5.mp3`

3. **Opdater svar i GameBoard.cs:**
   - Åbn `Models/GameModels/GameBoard.cs`
   - Ændre `Answer` property for hver Question til den rigtige sangtitel

## Eksempel på Opdatering i GameBoard.cs

```csharp
new Question { 
    Value = 100, 
    IsUsed = false, 
    MusicFile = "/music/pop1.mp3", 
    Answer = "Shape of You - Ed Sheeran"  // <-- Ændre dette til rigtig sangtitel
}
```

## Understøttede Formater
- MP3 (anbefalet)
- WAV
- OGG

## Bemærkninger
- Sørg for at filerne ikke er for store (anbefalet under 5MB per fil)
- Test at filerne afspiller korrekt før brug i spillet
- Hvis en musikfil mangler, vil spillet stadig virke, men ingen musik spilles for det spørgsmål

