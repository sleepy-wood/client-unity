using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SleepData
{
    public DateTime startDate;
    public DateTime endDate;
    public int type;
    public int userId;
    public string deletedAt;
    public int id;
    public string createdAt;
    public string updatedAt;
}

[Serializable]
public class ActivityData
{
    public int id;
    public double activeEnergyBurnedInKcal;
    public double activeEnergyBurnedGoalInKcal;
    public int exerciseTimeInMinutes;
    public int exerciseTimeGoalInMinutes;
    public int standHours;
    public int standHoursGoal;
    public DateTime date;
    public int userId;
    public string createdAt;
    public string updatedAt;

}