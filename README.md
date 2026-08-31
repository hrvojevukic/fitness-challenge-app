# Fitness Challenge

Full-stack aplikacija za bilježenje sportskih aktivnosti — trčanje, hodanje,
biciklizam, teretana, plivanje i dnevni koraci. Svaka aktivnost pretvara se u
jedinstveni bodovni rezultat, korisnici se rangiraju na globalnoj ljestvici, a
svaki korisnik ima osobni dashboard s grafovima aktivnosti kroz vrijeme i
udjelom aktivnosti po sportovima.

- **Backend:** C# / ASP.NET Core Web API (.NET 8), Entity Framework Core, SQLite
- **Frontend:** Angular 18, Chart.js

## Preduvjeti

- .NET 8 SDK
- Node.js 18+ i npm

## Pokretanje lokalno

Backend i frontend pokreću se u dva odvojena terminala.

### Backend

```bash
cd backend/src/FitnessChallenge.Api
dotnet run
```

Pokreće se na **http://localhost:5205**. Pri prvom pokretanju automatski kreira
lokalnu `fitness.db` SQLite datoteku i puni je primjerom podataka.

### Frontend

```bash
cd frontend
npm install
npm start
```

Pokreće se na **http://localhost:4200**.

### Korištenje

Otvori http://localhost:4200, registriraj korisnika pa se prijavi tim imenom i
prezimenom. Nakon prijave možeš bilježiti aktivnosti. ljestvica i
dashboard se osvježavaju kako se dodaju podaci.
