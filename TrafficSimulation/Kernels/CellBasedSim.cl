
#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable

#define uint unsigned int

#define MaxCarSize (10)
#define SpeedLimit (6) // 54 km/h
#define SlowdownProbability (0.2f)

#define CellNone (-1)
#define CellTerminator (-2)

typedef struct __attribute__((aligned(4))) Cell_t
{
    int T1, T2, T3, T4, T5;
    float P1, P2, P3, P4, P5;
    int JunctionIndex;
    int NearestJunctionIndex;
} Cell;

typedef struct __attribute__((aligned(4))) CellToCar_t
{
    int CarIndex;
} CellToCar;

typedef struct __attribute__((aligned(4))) Junction_t
{
    int CellIndex;
    int WaitingCount;
} Junction;

typedef struct __attribute__((aligned(4))) Generator_t
{
    int CellIndex;
    float ProbabilityLambda;
    float TimeLeft;
} Generator;

typedef struct __attribute__((aligned(4))) Car_t
{
    int Position;
    int Speed;
    int Size;
} Car;


void FindCarCells(
    global Cell* cells,
    global CellToCar* cellsToCar,
    int i,
    global Car* car,
    int* carCells
)
{
    int currentCell = 0;

    carCells[currentCell++] = car->Position;

    global Cell* cell = &cells[car->Position];
    
    int remaining = car->Size - 1;
    while (remaining > 0) {
        if (cell->T1 != CellNone && cellsToCar[cell->T1].CarIndex == i) {
            carCells[currentCell++] = cell->T1;
            cell = &cells[cell->T1];
        } else if (cell->T2 != CellNone && cellsToCar[cell->T2].CarIndex == i) {
            carCells[currentCell++] = cell->T2;
            cell = &cells[cell->T2];
        } else if (cell->T3 != CellNone && cellsToCar[cell->T3].CarIndex == i) {
            carCells[currentCell++] = cell->T3;
            cell = &cells[cell->T3];
        } else if (cell->T4 != CellNone && cellsToCar[cell->T4].CarIndex == i) {
            carCells[currentCell++] = cell->T4;
            cell = &cells[cell->T4];
        } else if (cell->T5 != CellNone && cellsToCar[cell->T5].CarIndex == i) {
            carCells[currentCell++] = cell->T5;
            cell = &cells[cell->T5];
        }
        
        remaining--;
    }

    carCells[currentCell] = CellNone;
}

int FindNextCell(
    global Cell* c,
    
    global float* random,
    const int randomLength,
    int randomIndex
)
{
    float rand = random[(uint)randomIndex % randomLength];

    float current = c->P1;
    if (rand <= current && c->T1 != CellNone) return c->T1;
    current += c->P2;
    if (rand <= current && c->T2 != CellNone) return c->T2;
    current += c->P3;
    if (rand <= current && c->T3 != CellNone) return c->T3;
    current += c->P4;
    if (rand <= current && c->T4 != CellNone) return c->T4;

    return c->T5;
}

void FillFrontCells(
    global Cell* cells,
    
    int carIdx, int* frontCells, int headCellIdx, int range,
    
    global float* random,
    const int randomLength,
    const int randomSeed
)
{
    for (int i = 0; i < range; i++) {
        global Cell* cell = &cells[headCellIdx];

        int idx = FindNextCell(cell, random, randomLength, randomSeed + carIdx * headCellIdx);
        if (idx == CellNone) {
            for (; i < SpeedLimit + 1; i++) {
                frontCells[i] = CellNone;
            }
            break;
        }

        frontCells[i] = idx;
        headCellIdx = idx;
    }
}

kernel void DoStepCar(
    global Cell* cells,
    global CellToCar* cellsToCar,
    const int cellsLength,
    global Junction* junctions,
    const int junctionsLength,
    global Car* cars,
    const int carsLength,
    
    global float* random,
    const int randomLength,
    const int randomSeed
)
{
    int i = get_global_id(0);
    if (i >= carsLength) {
        return;
    }

    global Car* car = &cars[i];
    
    if (car->Position == CellNone) {
        // Car is not spawned yet
        return;
    }
    
    int carCells[MaxCarSize];
    int frontCells[SpeedLimit + 1];

    // Find all cells occupied by car
    {
        FindCarCells(cells, cellsToCar, i, car, carCells);
        
        FillFrontCells(cells, i, frontCells, carCells[car->Size - 1], SpeedLimit + 1, random, randomLength, randomSeed);

        bool canAccelerate = true;
        int j = 0;
        for (; j < car->Speed; j++)
        {
            if (frontCells[j] == CellNone) {
                canAccelerate = false;
                car->Speed = j;
                break;
            }

            global CellToCar* c = &cellsToCar[frontCells[j]];
            if (c->CarIndex != CellNone) {
                canAccelerate = false;
                car->Speed = j;
                break;
            }
        }

        if (car->Speed < SpeedLimit && canAccelerate && frontCells[car->Speed] != CellNone) {
            global CellToCar* c = &cellsToCar[frontCells[car->Speed]];
            if (c->CarIndex == CellNone) {
                car->Speed++;
            }
        }

        // Random slowdown
        if (car->Speed >= 1) {
            float rand = random[(uint)(randomSeed + i * carCells[0]) % randomLength];
            if (rand < SlowdownProbability) {
                car->Speed--;
            }
        }
    }

    // Move car by specified cells
    for (int j = 0; j < car->Speed; j++) {
        int nextIdx = frontCells[j];
        
        global Cell* nextCell = &cells[nextIdx];
        global CellToCar* nextCellToCar = &cellsToCar[nextIdx];
        
        if (nextCell->JunctionIndex == CellTerminator) {
            // Car is at terminator, remove it
            car->Position = CellNone;
            car->Speed = 0;
            
            for (int j = 0; j < car->Size; j++) {
                cellsToCar[carCells[j]].CarIndex = CellNone;
            }
            break;
        }
        
        if (atomic_cmpxchg(&nextCellToCar->CarIndex, CellNone, i) != CellNone) {
            car->Speed = 0;
            break;
        }

        cellsToCar[carCells[0]].CarIndex = CellNone;

        if (car->Size > 1) {
            car->Position = carCells[1];
            
            for (int k = 1; k < car->Size; k++) {
                carCells[k - 1] = carCells[k];
            }
        } else {
            car->Position = nextIdx;
        }

        carCells[car->Size - 1] = nextIdx;
    }
    
    if (car->Speed == 0) {
        global Cell* headCell = &cells[carCells[car->Size - 1]];
        if (headCell->NearestJunctionIndex != CellNone) {
            global Junction* junction = &junctions[headCell->NearestJunctionIndex];
            atomic_inc(&junction->WaitingCount);
        }
    }
}


float RandomExp(
    float lambda,
    
    global float* random,
    const int randomLength,
    const int randomIndex
)
{
    float rand = random[(uint)randomIndex % randomLength];
    return -native_log(1 - rand) / lambda;
}

kernel void SpawnCars(
    global Cell* cells,
    global CellToCar* cellsToCar,
    const int cellsLength,
    global Generator* generators,
    const int generatorsLength,
    global Car* cars,
    const int carsLength,
    
    global float* random,
    const int randomLength,
    const int randomSeed
)
{
    int i = get_global_id(0);
    if (i >= generatorsLength) {
        return;
    }

    global Generator* generator = &generators[i];

    if (generator->TimeLeft > 0) {
        // One fixed step
        generator->TimeLeft -= 1;
        return;
    }
    
    generator->TimeLeft = RandomExp(generator->ProbabilityLambda, random, randomLength, randomSeed * (i + 1));
    
    if (cellsToCar[generator->CellIndex].CarIndex != CellNone) {
        // There is not enough space for new car
        return;
    }

    int carSize = 1 + (int)(random[(uint)(randomSeed * (i + 1) * 2) % randomLength] * 6);
    
    int frontCells[SpeedLimit + 1];
    FillFrontCells(cells, i, frontCells, generator->CellIndex, carSize - 1, random, randomLength, randomSeed);
    
    for (int j = 0; j < carSize - 1; j++) {
        if (frontCells[j] == CellNone || cellsToCar[frontCells[j]].CarIndex != CellNone) {
            // There is not enough space for new car
            return;
        }
    }
    
    int jo = (carsLength / 2) + (10 * i);
    for (int j = 0; j < carsLength; j++) {
        int carIdx = (jo + j) % carsLength;
        global Car* car = &cars[carIdx];
        
        if (atomic_cmpxchg(&car->Position, CellNone, generator->CellIndex) != CellNone) {
            // This car is already spawned
            continue;
        }

        car->Speed = 0;
        car->Size = carSize;
        
        cellsToCar[generator->CellIndex].CarIndex = carIdx;
        for (int k = 0; k < carSize - 1; k++) {
            cellsToCar[frontCells[k]].CarIndex = carIdx;
        }
        
        break;
    }
}