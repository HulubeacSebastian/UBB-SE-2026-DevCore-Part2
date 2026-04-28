// <copyright file="LoanDialogStateService.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Services;

public class LoanDialogStateService
{
    private const int PositiveThreshold = 0;

    public bool ShouldComputeEstimate(double desiredAmount, int preferredTermMonths, string purpose)
    {
        return desiredAmount > PositiveThreshold &&
               preferredTermMonths > PositiveThreshold &&
               !string.IsNullOrWhiteSpace(purpose);
    }
}