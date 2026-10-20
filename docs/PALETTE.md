# Palette ReBear — ordine di utilizzo

Tutti i 61 componenti, inclusi i 32 recuperati da Rhino2FEM, condividono lo stesso percorso nella scheda **ReBear**. I prefissi numerici ordinano i pannelli; le sezioni native di Grasshopper (`GH_Exposure`) ordinano le fasi all'interno dei pannelli. I componenti nella stessa fase sono strumenti alternativi o affini e mantengono l'ordinamento nativo di Grasshopper.

**Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display**

Lo Standard è accanto ai materiali perché serve per i loro valori di progetto. Coordinate, azioni e inviluppi delle azioni sono in Input; opzioni e costruzione del checker sono in Setup. I risultati di verifica e i relativi inviluppi sono in Results. I componenti storici non sono nascosti e conservano nomi, GUID e porte.

## 1 Materials — 7 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Standard | Storico | 1/1 |
| 2 | Database Concrete | Storico | 1/1 |
| 3 | Database Rebar Steel | Storico | 2/1 |
| 4 | Database Bar Steel | Storico | 2/1 |
| 4 | Database Tendon Steel | Storico | 2/1 |
| 5 | Concrete Material Design Value | Storico | 2/8 |
| 5 | Steel Material Design Value | Storico | 2/11 |

## 2 Sections — 12 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Rebar Element | Storico | 3/1 |
| 1 | Rebar Path Layout | Nuovo | 5/3 |
| 1 | Tendon Element | Storico | 4/1 |
| 2 | Circular RC Section | Nuovo | 8/6 |
| 2 | Rectangular RC Section | Nuovo | 10/6 |
| 3 | Box RC Section | Nuovo | 13/6 |
| 3 | I RC Section | Nuovo | 14/6 |
| 3 | T RC Section | Nuovo | 12/6 |
| 4 | Reinforced Concrete Section | Storico | 4/1 |
| 5 | Section Geometry Check | Nuovo | 4/4 |
| 6 | Concrete Mechanical Properties | Storico | 1/12 |
| 6 | Homogenized Mechanical Properties | Storico | 2/12 |

## 3 Input — 6 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Coordinate System | Storico | 4/1 |
| 1 | Secton Coordinate System | Storico | 1/1 |
| 2 | Force component | Storico | 9/1 |
| 3 | Load Envelope | Nuovo | 1/4 |
| 4 | Creep and Homogenization | Nuovo | 4/3 |
| 4 | Psi coefficient | Storico | 3/1 |

## 4 Setup — 7 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Concrete Checker Options | Storico | 8/1 |
| 2 | Concrete Checker | Storico | 3/1 |
| 3 | Shear Options | Nuovo | 12/1 |
| 4 | Torsion Options | Nuovo | 11/1 |
| 5 | SLE Options | Nuovo | 10/2 |
| 6 | Plate Detailing Options | Nuovo | 3/1 |
| 7 | Detailing Options | Nuovo | 15/1 |

## 5 Analysis — 4 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Stress Analysis | Storico | 2/2 |
| 2 | 3D Failure Domain Check | Storico | 1/3 |
| 3 | Domain Slice at N | Nuovo | 4/4 |
| 4 | Moment Curvature | Nuovo | 7/7 |

## 6 Checks — 12 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | 3D Failure Domain Analysis | Storico | 2/5 |
| 1 | Section Resistance Check | Nuovo | 3/1 |
| 2 | Shear Check v2 | Nuovo | 5/1 |
| 3 | Circular Section Shear Check | Storico | 9/6 |
| 3 | Rectangular Section Shear Check | Storico | 16/18 |
| 3 | Rectangular Section Shear Check No stirrup | Storico | 8/6 |
| 4 | Torsion and Shear | Nuovo | 6/1 |
| 5 | Serviceability Check v2 | Nuovo | 3/2 |
| 6 | Serviceability Crack Width Check | Storico | 7/6 |
| 6 | Serviceability Stress Check | Storico | 2/8 |
| 7 | Batch Section Checks | Nuovo | 9/3 |
| 8 | Reinforcement Candidate Search | Nuovo | 4/4 |

## 7 Detailing — 3 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Concrete Cover | Nuovo | 16/4 |
| 2 | Anchorage and Lap | Nuovo | 11/3 |
| 3 | Reinforcement Detailing | Nuovo | 4/2 |

## 8 Results — 6 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | Check Result | Nuovo | 1/11 |
| 2 | Effective Cracked Section | Nuovo | 1/6 |
| 2 | Print Stress Analysis Results | Storico | 1/26 |
| 3 | Secton Resistance Summary | Storico | 3/8 |
| 4 | Check Envelope | Nuovo | 1/5 |
| 5 | Check Report | Nuovo | 1/3 |

## 9 Display — 4 componenti

| Fase | Componente | Origine | Porte I/O |
|---:|---|---|---:|
| 1 | View Strain Analysis Results | Storico | 1/7 |
| 2 | Color Mesh | Storico | 4/1 |
| 2 | Result Color Mesh | Storico | 2/1 |
| 3 | Inspect Strain Analysis Results | Storico | 2/1 |

La disposizione è definita una sola volta in `src/ConcreteSectionChecker.Grasshopper/ComponentPalette.cs`. Per rigenerare questa pagina e l'indice dei nuovi componenti dopo una modifica, compilare la Release ed eseguire:

```powershell
dotnet run --project tests/ConcreteSectionChecker.Checks -c Release --no-build -- --write-palette
```
