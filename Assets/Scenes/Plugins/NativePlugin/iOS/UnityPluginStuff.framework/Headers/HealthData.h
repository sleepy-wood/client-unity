#ifndef HealthData_h
#define HealthData_h

typedef struct {
    double startDateInSeconds;
    double endDateInSeconds;
    int value;
} SleepSample;

typedef struct {
    double dateInSeconds;
    // _Bool isMoveMode;
    // double moveTimeInMinutes;
    // double moveTimeGoalInMinutes;
    double activeEnergyBurnedInKcal;
    double activeEnergyBurnedGoalInKcal;
    int exerciseTimeInMinutes;
    int exerciseTimeGoalInMinutes;
    int standHours;
    int standHoursGoal;
} ActivitySample;

#endif
