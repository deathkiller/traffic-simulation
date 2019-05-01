
#pragma OPENCL EXTENSION cl_khr_global_int32_base_atomics : enable

#define uint unsigned int

#define SpeedLimit (6.0f) // 54 km/h
#define SlowdownProbability (0.2f)

#define CellNone (-1)
#define CellTerminator (-2)

#define CellUnlocked (0)
#define CellLocked (1)


typedef struct __attribute__((aligned(4))) Cell_t
{
    int T1, T2, T3, T4, T5;
    float P1, P2, P3, P4, P5;
    int JunctionIndex;
    int NearestJunctionIndex;
    
    float Length;
    int Lock;
} Cell;

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
    float PositionInCell;
    float Speed;
    int Size;
    int AlreadyProcessed;
} Car;


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

bool TryLockCell(
    global Cell* cells,
    int cellIdx
)
{
    return (atomic_xchg(&cells[cellIdx].Lock, CellLocked) == CellUnlocked);
}

void UnlockCell(
    global Cell* cells,
    int cellIdx
)
{
    atomic_xchg(&cells[cellIdx].Lock, CellUnlocked);
}

bool RemoveFromCellsToCar(
    global Cell* cells,
    global int* cellsToCar,
    const int carsPerCell,
    
    int cellIdx,
    int position
)
{
    if (!TryLockCell(cells, cellIdx)) {
        return false;
    }

    for (int i = position; i < carsPerCell - 1; i++) {
        int idx = cellIdx * carsPerCell + i;
        cellsToCar[idx] = cellsToCar[idx + 1];
        if (cellsToCar[idx] == CellNone) {
            UnlockCell(cells, cellIdx);
            return true;
        }
    }

    cellsToCar[cellIdx * carsPerCell + carsPerCell - 1] = CellNone;
    UnlockCell(cells, cellIdx);
    return true;
}

void CarFollowing(
    global Car* cars,
    
    global float* random,
    const int randomLength,
    int randomSeed,
    
    const float dt,

    int followerIdx,
    int leaderIdx
)
{
    global Car* follower = &cars[followerIdx];
    if (leaderIdx == -1) {
        follower->Speed = min(follower->Speed + dt, SpeedLimit);
        return;
    }

    global Car* leader = &cars[leaderIdx];
    
    float gap = (leader->PositionInCell - leader->Size) - follower->PositionInCell;
    if (gap < 0) {
        follower->Speed = 0;
    } else {
        follower->Speed = min(min(follower->Speed + dt, gap), SpeedLimit);
    }
    
    if (follower->Speed >= 1) {
        float rand = random[(uint)(randomSeed + followerIdx) % randomLength];
        if (rand < SlowdownProbability) {
            follower->Speed -= 1.0f;
        }
    }
}

kernel void DoStepCarPre(
    global Cell* cells,
    global int* cellsToCar,
    const int cellsLength,
    global Junction* junctions,
    const int junctionsLength,
    global Car* cars,
    const int carsLength,
    const int carsPerCell,
    
    global float* random,
    const int randomLength,
    const int randomSeed,
    
    const float dt
)
{
    int i = get_global_id(0);
    if (i >= cellsLength) {
        return;
    }

    for (int j = 0; j < carsPerCell; j++) {
        int carIdx = cellsToCar[i * carsPerCell + j];
        if (carIdx == CellNone) {
            break;
        }
    
        global Car* car = &cars[carIdx];
        
        car->AlreadyProcessed = 0;
        
        car->PositionInCell += car->Speed * dt;
        
        if (car->Speed < 0.1f) {
            global Cell* headCell = &cells[i];
            if (headCell->NearestJunctionIndex != CellNone) {
                global Junction* junction = &junctions[headCell->NearestJunctionIndex];
                atomic_inc(&junction->WaitingCount);
            }
        }
        
        int leaderIdx;
        if (j == 0) {
            leaderIdx = -1;
        } else {
            leaderIdx = cellsToCar[i * carsPerCell + (j - 1)];
        }
        
        CarFollowing(cars, random, randomLength, randomSeed, dt, carIdx, leaderIdx);
    }
}

kernel void DoStepCarPost(
    global Cell* cells,
    global int* cellsToCar,
    const int cellsLength,
    global Junction* junctions,
    const int junctionsLength,
    global Car* cars,
    const int carsLength,
    const int carsPerCell,
    
    global float* random,
    const int randomLength,
    const int randomSeed,
    
    global int* isChanged
)
{
    int i = get_global_id(0);
    if (i >= cellsLength) {
        return;
    }
    
    for (int j = 0; j < carsPerCell; j++) {
        int carIdx = cellsToCar[i * carsPerCell + j];
        if (carIdx == CellNone) {
            break;
        }
        
        global Car* car = &cars[carIdx];
        
        if (atomic_xchg(&car->AlreadyProcessed, 1) != 0) {
            continue;
        }
        
        if (car->PositionInCell >= cells[i].Length) {
            // Car is past the current cell
            int nextIdx = FindNextCell(&cells[i], random, randomLength, (uint)(randomSeed + i + j));
            global Cell* nextCell = &cells[nextIdx];
        
            if (nextCell->JunctionIndex == CellTerminator) {
                // Car is at terminator, remove it
                if (!RemoveFromCellsToCar(cells, cellsToCar, carsPerCell, i, j)) {
                    atomic_xchg(&car->AlreadyProcessed, 0);
                    atomic_xchg(isChanged, 1);
                    continue;
                }
                
                j--;

                car->Position = CellNone;
                car->PositionInCell = 0;
                car->Speed = 0;
                continue;
            }

            if (!TryLockCell(cells, nextIdx)) {
                atomic_xchg(&car->AlreadyProcessed, 0);
                atomic_xchg(isChanged, 1);
                continue;
            }
            
            bool success = false;
            for (int k = 0; k < carsPerCell; k++) {
                int idx = nextIdx * carsPerCell + k;
                if (cellsToCar[idx] == CellNone) {
                    cellsToCar[idx] = carIdx;

                    float additionalDistance = (car->PositionInCell - cells[i].Length);
                    if (k == 0) {
                        // Lane is empty
                        if (!RemoveFromCellsToCar(cells, cellsToCar, carsPerCell, i, j)) {
                            success = true;
                            cellsToCar[idx] = CellNone;
                            break;
                        }
                
                        j--;

                        car->Position = nextIdx;
                        car->PositionInCell = additionalDistance;

                        success = true;
                    } else {
                        global Car* leader = &cars[cellsToCar[idx - 1]];
                        float gap = (leader->PositionInCell - leader->Size);

                        if (additionalDistance <= gap) {
                            // Gap between vehicles is big enough
                            if (!RemoveFromCellsToCar(cells, cellsToCar, carsPerCell, i, j)) {
                                success = true;
                                cellsToCar[idx] = CellNone;
                                break;
                            }
                            
                            j--;

                            car->Position = nextIdx;
                            car->PositionInCell = additionalDistance;

                            success = true;
                        } else if (gap >= 0) {
                            // Vehicle must decelerate
                            if (!RemoveFromCellsToCar(cells, cellsToCar, carsPerCell, i, j)) {
                                success = true;
                                cellsToCar[idx] = CellNone;
                                break;
                            }
                        
                            j--;

                            car->Position = nextIdx;
                            car->PositionInCell = gap;
                            car->Speed = min(car->Speed, gap);

                            success = true;
                        } else {
                            // There is not enough space, transfer is not possible
                            // Do rollback
                            cellsToCar[idx] = CellNone;
                        }
                    }
                    break;
                }
            }
            
            UnlockCell(cells, nextIdx);

            if (success) {
                // Current cell has changed
                atomic_xchg(&car->AlreadyProcessed, 0);
                atomic_xchg(isChanged, 1);
            } else {
                // Current cell is still the same, next cell is blocked
                car->Speed = 0;
            }
        } else {
            // Car is still in the current cell
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
    global int* cellsToCar,
    const int cellsLength,
    global Generator* generators,
    const int generatorsLength,
    global Car* cars,
    const int carsLength,
    const int carsPerCell,
    
    global float* random,
    const int randomLength,
    const int randomSeed,
    
    const float dt
)
{
    int i = get_global_id(0);
    if (i >= generatorsLength) {
        return;
    }

    global Generator* generator = &generators[i];

    if (generator->TimeLeft > 0) {
        generator->TimeLeft -= dt;
        return;
    }
    
    generator->TimeLeft = RandomExp(generator->ProbabilityLambda, random, randomLength, randomSeed * (i + 1));

    int carSize = 1 + (int)(random[(uint)(randomSeed * (i + 1) * 2) % randomLength] * 6);

    int jo = (carsLength / 2) + (10 * i);
    for (int j = 0; j < carsLength; j++) {
        int carIdx = (jo + j) % carsLength;
        global Car* car = &cars[carIdx];
        
        if (atomic_cmpxchg(&car->Position, CellNone, generator->CellIndex) != CellNone) {
            // This car is already spawned
            continue;
        }
        
        bool success = false;
        for (int k = 0; k < carsPerCell; k++) {
            int idx = generator->CellIndex * carsPerCell + k;
            // Cell is owned only by one generator, no locking is needed
            if (cellsToCar[idx] == CellNone) {
                cellsToCar[idx] = carIdx;

                if (k == 0) {
                    // Lane is empty
                    success = true;
                } else {
                    global Car* leaderCar = &cars[cellsToCar[idx - 1]];
                    float gap = (leaderCar->PositionInCell - leaderCar->Size);
                    success = (gap >= 0);
                }

                if (success) {
                    car->Position = generator->CellIndex;
                    car->PositionInCell = 0;
                    car->Speed = 0;
                    car->Size = carSize;
                    car->AlreadyProcessed = 1;
                } else {
                    // There is not enough space, spawn is not possible
                    // Do rollback
                    cellsToCar[idx] = CellNone;
                }
                break;
            }
        }
        
        if (!success) {
            // There is not enough space, spawn is not possible
            // Do rollback
            atomic_xchg(&car->Position, CellNone);
        }

        break;
    }
}