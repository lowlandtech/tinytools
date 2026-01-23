# LowlandTech.Specs

A BDD (Behavior-Driven Development) testing framework with executable specifications integrated with xUnit v3.

## Features

- **Gherkin-style Scenarios**: Write tests using Given/When/Then patterns
- **Executable Specifications**: Attributes carry structured, machine-readable data
- **xUnit Integration**: Seamlessly runs with `dotnet test`
- **Auto-Execution**: Optional auto-apply for preconditions and validations
- **Trait Support**: Filter tests by UAT identifiers
- **Valid Gherkin Output**: `ToString()` produces readable specifications

## Quick Start

### Manual Approach (Traditional)

```csharp
[Scenario(SpecId = "SPEC0001", UserStory = "US01", ScenarioId = "SC01", 
          Label = "Adding to account balance")]
public class WhenDepositingManually : WhenTestingFor<AccountReducer>
{
    private AccountState _state;

    [For(StateType = typeof(AccountState))]
    protected override AccountReducer For() => new AccountReducer();

    [Given(Property = "Balance", Operator = "Equals", Value = 100)]
    protected override void Given()
    {
        _state = new AccountState { Balance = 100 };
    }

    [When(Action = "depositing funds", ActionType = "DEPOSIT")]
    protected override void When()
    {
        _state = Sut.Reduce(_state, new DepositAction(50));
    }

    [Then(Uat = "01", Comment = "Balance should increase",
          Property = "Balance", Operator = "Equals", Expected = 150)]
    public void ShouldUpdateBalance()
    {
        Assert.Equal(150, _state.Balance);
    }
}
```

### Auto-Execution Approach

```csharp
public class WhenDepositingAuto : WhenTestingFor<AccountReducer>
{
    private AccountState _state;
    
    protected override bool AutoExecuteGivens => true;

    [Given(Property = "Balance", Value = 100)]
    [Given(Property = "AccountStatus", Value = "Active")]
    protected override void Given()
    {
        _state = new AccountState();
        // Framework auto-applies: Balance = 100, AccountStatus = "Active"
    }

    [When(Action = "deposit", ActionType = "DEPOSIT")]
    protected override void When()
    {
        _state = Sut.Reduce(_state, new DepositAction(50));
    }

    [Then(Uat = "01", Property = "Balance", Operator = "Equals", Expected = 150)]
    public void ShouldUpdateBalance()
    {
        this.AutoValidate(); // That's it!
    }
}
```

## Gherkin Output

Calling `ToString()` on attributes produces valid Gherkin:

```
@SPEC0001 @US01 @SC01
Scenario: Adding to account balance
  # For: AccountState
  Given Balance Equals 100
  And AccountStatus Equals "Active"
  When depositing funds (DEPOSIT)
  #UAT-01: Balance should increase
  Then Balance Equals 150
    And TransactionCount Equals 1
```

## Attributes

| Attribute | Target | Properties |
|-----------|--------|------------|
| `[Scenario]` | Class | `SpecId`, `UserStory`, `UseCase`, `ScenarioId`, `Label` |
| `[For]` | Method | `StateType` |
| `[Given]` | Method | `Property`, `Operator`, `Value` (multi-attribute) |
| `[When]` | Method | `Action`, `ActionType` |
| `[Then]` | Method | `Uat`, `Comment`, `Assertion`, `Property`, `Operator`, `Expected`, Postconditions |

## Supported Operators

- `Equals`, `NotEquals`
- `GreaterThan`, `GreaterThanOrEquals`
- `LessThan`, `LessThanOrEquals`
- `Contains`
- `IsNull`, `IsNotNull`

## Installation

```bash
dotnet add package LowlandTech.Specs
```

## Running Tests

```bash
dotnet test

# Filter by UAT
dotnet test --filter "Uat=01"
```

## License

MIT
