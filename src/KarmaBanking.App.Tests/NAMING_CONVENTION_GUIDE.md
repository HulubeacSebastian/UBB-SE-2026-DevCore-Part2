# Test Naming Convention Quick Reference

## Standard Format
```
[MethodName]_When[Condition]_Then[ExpectedResult]
```

## Components

### 1. MethodName
The name of the method being tested.
- Use PascalCase
- Example: `CreateAccountAsync`, `GetRepaymentProgress`, `PayInstallmentAsync`

### 2. When[Condition]
Describes the circumstances or preconditions under which the test runs.
- Starts with "When"
- Describes the input, state, or scenario
- Examples:
  - `WhenAmountIsNegative`
  - `WhenFileIsNull`
  - `WhenValidLoanProvided`
  - `WhenGivenVariousInputs`
  - `WhenStandardPaymentMade`
  - `WhenSelectedTickerIsAll`

### 3. Then[ExpectedResult]
Describes what the test expects to happen as a result.
- Starts with "Then"
- Describes the outcome, assertion, or expected behavior
- Examples:
  - `ThenThrowsArgumentException`
  - `ThenReturnsExpectedProgress`
  - `ThenUpdatesBalanceAndRemainingMonths`
  - `ThenCallsLoanServiceAndReturnsLoansList`
  - `ThenMarksOnlyOneCurrentInstallment`

## Real-World Examples

### Example 1: Simple Validation Test
```csharp
// Before
[Fact]
public void ValidateFile_NullFile_ReturnsFalseAndErrorMessage()
{
    // ...
}

// After
[Fact]
public async Task ValidateFileAsync_WhenFileIsNull_ThenReturnsFalseAndErrorMessage()
{
    // ...
}
```

### Example 2: Exception Test
```csharp
// Before
[Fact]
public async Task DepositAsync_NegativeAmount_ThrowsArgumentException()
{
    // ...
}

// After
[Fact]
public async Task DepositAsync_WhenAmountIsNegative_ThenThrowsArgumentException()
{
    // ...
}
```

### Example 3: Complex Logic Test
```csharp
// Before
[Fact]
public async Task ProcessApplicationStatusAsync_WhenUserHasFiveActiveLoans_RejectsApplication()
{
    // ...
}

// After
[Fact]
public async Task ProcessApplicationStatusAsync_WhenUserHasFiveActiveLoans_ThenRejectsApplication()
{
    // ...
}
```

### Example 4: Theory Test with Conditions
```csharp
// Before
[Theory]
[InlineData(150.75, true, 150.75)]
public void TryParsePositiveAmount_ReturnsExpectedResult(...)
{
    // ...
}

// After
[Theory]
[InlineData(150.75, true, 150.75)]
public void TryParsePositiveAmount_WhenGivenVariousInputs_ThenReturnsExpectedResult(...)
{
    // ...
}
```

## Guidelines

### Do's ✓
- Use clear, descriptive condition and result names
- Use "When" prefix for conditions consistently
- Use "Then" prefix for results consistently
- Be specific about what's being tested
- Keep names readable (avoid extremely long names by being concise)
- Use domain-specific terms from the codebase
- Describe the actual behavior being tested

### Don'ts ✗
- Avoid vague conditions like `WhenCalled` (only if absolutely generic)
- Avoid misleading results that don't match actual test assertions
- Don't mix "And" or "Or" in the When/Then parts (use underscores or combine them)
- Avoid abbreviations that aren't clear
- Don't duplicate the method name in the condition
- Avoid tense inconsistencies (use consistent tense)

## Common Patterns

### Null/Empty Input Tests
```
[Method]_WhenInputIsNull_ThenThrowsException
[Method]_WhenStringIsEmpty_ThenReturnsFalse
[Method]_WhenCollectionIsEmpty_ThenReturnsEmptyList
```

### Validation Tests
```
[Method]_WhenInputIsInvalid_ThenThrowsArgumentException
[Method]_WhenConditionFails_ThenReturnsFalse
[Method]_WhenRequiredFieldMissing_ThenReturnsErrors
```

### Success Path Tests
```
[Method]_WhenValidInputProvided_ThenReturnsExpectedResult
[Method]_WhenAllConditionsPass_ThenCreatesResource
[Method]_WhenCalled_ThenUpdatesStateCorrectly
```

### Edge Case Tests
```
[Method]_WhenValueExceedsMaximum_ThenCapsToBoundary
[Method]_WhenDateIsInPast_ThenThrowsArgumentException
[Method]_WhenResourceAlreadyClosed_ThenThrowsException
```

## Theory Tests with InlineData

For theory tests with multiple data cases, describe the general condition being tested:

```csharp
[Theory]
[InlineData(100, 1000, 10, true, 0, 900, 9)]
[InlineData(100, 1000, 10, false, 200, 800, 8)]
public void CalculatePaymentPreview_WhenGivenVariousPaymentModes_ThenReturnsExpectedValues(...)
{
    // Specific values are handled by InlineData annotations
}
```

## Benefits of This Convention

1. **Self-Documenting**: Test name explains purpose without reading implementation
2. **Searchable**: Easy to find tests for a specific condition
3. **Maintainable**: Developers can quickly understand test scope
4. **Predictable**: Consistent naming makes codebase navigable
5. **Professional**: Follows industry best practices
6. **Scalable**: Works well as test suite grows

## Enforcement

- Use code review to ensure compliance
- Implement naming rule in team's coding standards
- Use automated tools if available to flag non-compliant names
- Update existing tests following this pattern when they're modified
