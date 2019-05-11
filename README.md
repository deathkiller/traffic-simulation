<h2 align="center">
    Utilization of GPU for Parallel Simulation Computations
</h2>

<div align="center">
    Využití GPU pro paralelní simulační výpočty
</div>

<div align="center">
  <sub>
    Brought to you by <a href="https://github.com/deathkiller">@deathkiller</a>
  </sub>
</div>
<hr/>

[![Build Status](https://img.shields.io/appveyor/ci/deathkiller/traffic-simulation.svg?logo=visual-studio-code&logoColor=ffffff)](https://ci.appveyor.com/project/deathkiller/traffic-simulation)
[![Tests](https://img.shields.io/appveyor/tests/deathkiller/traffic-simulation.svg?compact_message)](https://ci.appveyor.com/project/deathkiller/traffic-simulation/build/tests)
[![Code Quality](https://img.shields.io/codacy/grade/374e7485fe0c42d4924fe4a2c151cf3a.svg)](https://www.codacy.com/app/deathkiller/traffic-simulation)
[![License](https://img.shields.io/github/license/deathkiller/traffic-simulation.svg)](https://github.com/deathkiller/traffic-simulation/blob/master/LICENSE)
[![Lines of Code](https://img.shields.io/badge/lines%20of%20code-15k-blue.svg)](https://github.com/deathkiller/traffic-simulation/graphs/code-frequency)


The purpose of this thesis is to implement road traffic simulation that uses a GPU for computations and compare simulation run time to a reference implementation that uses a CPU for computations.

The created application allows generating a road network for simulation using model with cellular automaton or car following model. Also, it can compute individual steps of the simulation using a GPU or a CPU. Simulation state can be monitored using simple visualization.

The application is written in C# programming language and uses .NET Framework. The application is using OpenCL technology and NOpenCL library to run computations on a GPU.

#### In Czech
Tato práce se zabývá implementací simulace silniční dopravy, která pro výpočty využívá jednotku GPU, a porovnáním doby běhu výpočtů simulace s referenční implementací, která pro výpočty využívá jednotku CPU.

Vytvořená aplikace umožňuje vygenerovat silniční síť pro simulaci s využitím modelu s celulárním automatem nebo car following modelu a následně počítat jednotlivé kroky simulace s pomocí jednotky GPU nebo CPU. Stav simulace je možné sledovat pomocí jednoduché vizualizace.

Aplikace je napsaná v jazyce C# a využívá technologii .NET Framework. Pro spouštění výpočtu na jednotce GPU se využívá technologie OpenCL a knihovna NOpenCL.


## License
This project is licensed under the terms of the [GNU General Public License v3.0](./LICENSE).

Uses [NOpenCL](https://github.com/tunnelvisionlabs/NOpenCL) licensed under the terms of the [MIT License](https://github.com/tunnelvisionlabs/NOpenCL/blob/master/LICENSE.md).

Uses [NBench](https://github.com/petabridge/NBench) licensed under the terms of the [Apache License 2.0](https://github.com/petabridge/NBench/blob/dev/LICENSE).