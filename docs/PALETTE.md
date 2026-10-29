# Palette ReBear 0.3 — ordine di utilizzo

Tutti i 58 componenti condividono il modello `ReBearComponent` e lo stesso percorso nella scheda **ReBear**. I prefissi numerici ordinano i pannelli; le sezioni native di Grasshopper (`GH_Exposure`) ordinano le fasi all'interno dei pannelli. I componenti nella stessa fase sono strumenti alternativi o affini.

**Materials → Sections → Input → Setup → Analysis → Checks → Detailing → Results → Display**

La colonna GUID indica se il componente conserva il `ComponentGuid` di 0.2.2 (porte identiche) o ne ha uno nuovo (porte o significato cambiati, vedere [MIGRATION.md](MIGRATION.md)). La fotografia completa di nomi, porte e GUID è `tests/Compatibility/palette-0.3.json`.

## 1 Materials — 7 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Standard | 0.2.2 | 1/1 |
| 2 | Database Concrete | 0.2.2 | 1/1 |
| 3 | Database Rebar Steel | 0.2.2 | 2/1 |
| 4 | Database Bar Steel | 0.2.2 | 2/1 |
| 4 | Database Tendon Steel | 0.2.2 | 2/1 |
| 5 | Concrete Material Design Value | nuovo 0.3 | 2/8 |
| 5 | Steel Material Design Value | nuovo 0.3 | 2/11 |

## 2 Sections — 12 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Rebar Element | nuovo 0.3 | 3/1 |
| 1 | Rebar Path Layout | 0.2.2 | 5/3 |
| 1 | Tendon Element | nuovo 0.3 | 4/1 |
| 2 | Circular RC Section | 0.2.2 | 8/6 |
| 2 | Rectangular RC Section | 0.2.2 | 10/6 |
| 3 | Box RC Section | 0.2.2 | 13/6 |
| 3 | I RC Section | 0.2.2 | 14/6 |
| 3 | T RC Section | 0.2.2 | 12/6 |
| 4 | Reinforced Concrete Section | 0.2.2 | 4/1 |
| 5 | Section Geometry Check | 0.2.2 | 4/4 |
| 6 | Concrete Mechanical Properties | nuovo 0.3 | 1/12 |
| 6 | Homogenized Mechanical Properties | nuovo 0.3 | 2/12 |

## 3 Input — 6 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Coordinate System | 0.2.2 | 4/1 |
| 1 | Section Coordinate System | nuovo 0.3 | 1/1 |
| 2 | Force component | 0.2.2 | 9/1 |
| 3 | Load Envelope | 0.2.2 | 1/4 |
| 4 | Creep and Homogenization | 0.2.2 | 4/3 |
| 4 | Psi coefficient | 0.2.2 | 3/1 |

## 4 Setup — 9 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Concrete Checker Options | nuovo 0.3 | 9/1 |
| 2 | Concrete Checker | 0.2.2 | 3/1 |
| 3 | Circular Shear Options | nuovo 0.3 | 12/1 |
| 3 | Rectangular Shear Options | nuovo 0.3 | 18/2 |
| 3 | Shear Options | 0.2.2 | 12/1 |
| 4 | Torsion Options | 0.2.2 | 11/1 |
| 5 | SLE Options | 0.2.2 | 10/2 |
| 6 | Plate Detailing Options | 0.2.2 | 3/1 |
| 7 | Detailing Options | 0.2.2 | 15/1 |

## 5 Analysis — 4 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Stress Analysis | 0.2.2 | 2/2 |
| 2 | 3D Failure Domain Check | 0.2.2 | 1/3 |
| 3 | Domain Slice at N | 0.2.2 | 4/4 |
| 4 | Moment Curvature | 0.2.2 | 7/7 |

## 6 Checks — 7 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | 3D Failure Domain Analysis | nuovo 0.3 | 3/4 |
| 1 | Section Resistance Check | 0.2.2 | 3/1 |
| 2 | Shear Check | 0.2.2 | 5/1 |
| 3 | Torsion and Shear | 0.2.2 | 6/1 |
| 4 | Serviceability Check | 0.2.2 | 3/2 |
| 5 | Batch Section Checks | 0.2.2 | 9/3 |
| 6 | Reinforcement Candidate Search | 0.2.2 | 4/4 |

## 7 Detailing — 3 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Concrete Cover | nuovo 0.3 | 19/4 |
| 2 | Anchorage and Lap | 0.2.2 | 11/3 |
| 3 | Reinforcement Detailing | 0.2.2 | 4/2 |

## 8 Results — 6 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | Check Result | 0.2.2 | 1/11 |
| 2 | Effective Cracked Section | 0.2.2 | 1/6 |
| 2 | Stress Analysis Summary | nuovo 0.3 | 1/25 |
| 3 | Section Resistance Summary | 0.2.2 | 3/8 |
| 4 | Check Envelope | 0.2.2 | 1/5 |
| 5 | Check Report | 0.2.2 | 1/3 |

## 9 Display — 4 componenti

| Fase | Componente | GUID | Porte I/O |
|---:|---|---|---:|
| 1 | View Strain Analysis Results | 0.2.2 | 1/7 |
| 2 | Color Mesh | 0.2.2 | 4/1 |
| 2 | Result Color Mesh | 0.2.2 | 2/1 |
| 3 | Inspect Strain Analysis Results | 0.2.2 | 2/1 |

La disposizione è definita una sola volta in `src/ConcreteSectionChecker.Grasshopper/ComponentPalette.cs`. Per rigenerare questa pagina e il catalogo delle porte dopo una modifica, compilare la Release ed eseguire:

```powershell
dotnet obj/bin/ConcreteSectionChecker.Checks/Release/net8.0-windows/ConcreteSectionChecker.Checks.dll --write-palette
```
