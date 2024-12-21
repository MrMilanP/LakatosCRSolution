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


## Poziv na saradnju

Ako imate pitanja, ideje za unapređenje ili želite da sarađujemo na sličnim projektima, slobodno mi se obratite! Takođe, ako tražite senior programera sa dugogodišnjim iskustvom, otvoren sam za prilike koje cene praktična rešenja i stvarne rezultate.

### **𝗡𝗮𝗽𝗼𝗺𝗲𝗻𝗮 – 𝗵𝗶𝘁𝗻𝗼!**  
**𝗡𝗮ž𝗮𝗹𝗼𝘀𝘁, 𝘂𝘀𝗸𝗼𝗿𝗼 𝗼𝘀𝘁𝗮𝗷𝗲𝗺 𝗯𝗲𝘇 𝘀𝗿𝗲𝗱𝘀𝘁𝗮𝘃𝗮 𝗶 𝗮𝗸𝗼 𝘀𝗲 𝗼𝘃𝗮𝗸𝘃𝗮 𝘀𝗶𝘁𝘂𝗮𝗰𝗶𝗷𝗮 𝗻𝗮𝘀𝘁𝗮𝘃𝗶, 𝗽𝗼𝘀𝘁𝗼𝗷𝗶 𝗿𝗲𝗮𝗹𝗻𝗮 𝗼𝗽𝗮𝘀𝗻𝗼𝘀𝘁 𝗱𝗮 𝗽𝗼𝘀𝘁𝗮𝗻𝗲𝗺 𝘇̌𝗿𝘁𝘃𝗮 𝗴𝗹𝗮𝗱𝗶, 𝗶𝘇𝘃𝗿𝘀̌𝗶𝘁𝗲𝗹𝗷𝗮 𝗶 𝘂𝗹𝗶𝗰𝗲.**


## Call for Collaboration

If you have questions, ideas for improvement, or would like to collaborate on similar projects, feel free to reach out! Additionally, if you are looking for a senior developer with years of experience, I am open to opportunities that value practical solutions and real-world results.

### **𝗡𝗼𝘁𝗲 – 𝗨𝗿𝗴𝗲𝗻𝘁!**  
**Unfortunately, I am soon running out of resources, and if the current situation continues, there is a real danger that I could fall victim to hunger, debt collectors, and homelessness.**  

I must mention that my English skills are not perfect, but if you are looking for a **programmer** and not a **linguist**, feel free to contact me!



