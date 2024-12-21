# LakatosCRSolution

Ovo je .NET 8 rešenje (Visual Studio 2022) koje se sastoji od tri dela:

1. **LakatosCardReader (Class Library)**  
   - Glavna logika za čitanje i detekciju kartica (lična, saobraćajna, zdravstvena – čitanje zdravstvene je TODO).  
   - Deljena je i korišćena u ConsoleApp i Web projektima.

2. **ConsoleApp (CMD Test)**  
   - Demonstrira funkcionalnost čitača kartica u konzolnom okruženju.  
   - Pokrećete ga sa:
     ```bash
     dotnet run --project ConsoleApp/ConsoleApp.csproj
     ```

3. **Web (MVC + SignalR)**  
   - Prikazuje događaje umetanja/uklanjanja kartice u realnom vremenu.  
   - Nudi UI za čitanje i prikaz detalja kartice.  
   - Pokrećete ga sa:
     ```bash
     dotnet run --project Web/Web.csproj
     ```
   - Nakon pokretanja, u `launchSettings.json` su definisani portovi:
     ```json
     {
       "profiles": {
         "http": {
           "applicationUrl": "http://localhost:5054"
         },
         "https": {
           "applicationUrl": "https://localhost:7272;http://localhost:5054"
         }
       }
     }
     ```
   - Otvorite web browser na `https://localhost:7272` ili `http://localhost:5054`.

## Zašto .NET 8?

- Omogućava rad na Windows i Linux platformama.
- Pruža napredne performanse i dugoročnu podršku.

## Build i Pokretanje

1. **Build celog rešenja:**
   ```bash
   dotnet build

## Pokretanje ConsoleApp
```bash
dotnet run --project ConsoleApp/ConsoleApp.csproj
```
## Pokretanje Web aplikacije

```bash
dotnet run --project Web/Web.csproj
````

## Pristupanje Web aplikaciji

Otvorite [http://localhost:5054](http://localhost:5054) ili [https://localhost:7272](https://localhost:7272), izaberite čitač i startujte monitoring.

## Kratka Napomena o Implementaciji

- **Class Library (LakatosCardReader)**: Sadrži interfejse, modele, parsere i util klase za obradu kartica.
- **ConsoleApp**: Koristi `LakatosCardReader` direktno za demonstraciju rada u konzoli.
- **Web**: Koristi SignalR za emitovanje događaja i prikaz čitanja kartica u realnom vremenu.
- **Zdravstvena kartica**: Trenutno se samo detektuje; čitanje je planirano (*TODO*).

