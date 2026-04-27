// <copyright file="DecimalToTrendSymbolConverter.cs" company="Dev Core">
// Copyright (c) Dev Core. All rights reserved.
// </copyright>

namespace KarmaBanking.App.Utils;

using System;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// Converts a decimal value to a trend symbol (up or down arrow) for UI indication.
/// </summary>
public class DecimalToTrendSymbolConverter : IValueConverter
{
    private const decimal ZeroAmount = 0m;
    private const string PositiveTrendSymbol = "▲";
    private const string NegativeTrendSymbol = "▼";

    /// <summary>
    /// Converts a decimal value to a directional arrow string.
    /// </summary>
    /// <param name="value">The decimal value to evaluate.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>An up arrow for positive, down arrow for negative, or empty string otherwise.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal amount)
        {
            if (amount > ZeroAmount)
            {
                return PositiveTrendSymbol;
            }

            if (amount < ZeroAmount)
            {
                return NegativeTrendSymbol;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts a trend symbol back to a decimal. This method is not implemented.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotImplementedException">Always thrown as two-way binding is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}