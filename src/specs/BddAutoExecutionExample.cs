namespace LowlandTech.Specs;

/// <summary>
/// Example usage of the BDD auto-execution framework.
/// </summary>
/// <remarks>
/// This example demonstrates three approaches:
/// 1. Manual setup and assertions (traditional)
/// 2. Auto-execute Givens with manual assertions
/// 3. Full auto-execution with attribute-driven validation
/// </remarks>
public class BddAutoExecutionExample
{
    // Example state record
    public record AccountState
    {
        public decimal Balance { get; set; }
        public string AccountStatus { get; set; } = "Active";
        public int TransactionCount { get; set; }
    }

    // Example reducer/SUT
    public class AccountReducer
    {
        public AccountState Reduce(AccountState state, object action)
        {
            return action switch
            {
                DepositAction deposit => state with 
                { 
                    Balance = state.Balance + deposit.Amount,
                    TransactionCount = state.TransactionCount + 1
                },
                _ => state
            };
        }
    }

    public record DepositAction(decimal Amount);

    // APPROACH 1: Manual (Traditional)
    [Scenario(SpecId = "SPEC0001", UserStory = "US01", ScenarioId = "SC01", 
              Label = "Manual deposit test")]
    public class WhenDepositingManually : WhenTestingFor<AccountReducer>
    {
        private AccountState _state = null!;

        [For(StateType = typeof(AccountState))]
        protected override AccountReducer For() => new AccountReducer();

        [Given(Property = "Balance", Operator = "Equals", Value = 100)]
        [Given(Property = "AccountStatus", Operator = "Equals", Value = "Active")]
        protected override void Given()
        {
            // Manual setup
            _state = new AccountState { Balance = 100, AccountStatus = "Active" };
        }

        [When(Action = "depositing funds", ActionType = "DEPOSIT")]
        protected override void When()
        {
            _state = Sut.Reduce(_state, new DepositAction(50));
        }

        [Then(Uat = "01", Comment = "Balance should increase by deposit amount",
              Property = "Balance", Operator = "Equals", Expected = 150)]
        public void ShouldUpdateBalance()
        {
            // Manual assertion
            if (_state.Balance != 150)
                throw new InvalidOperationException($"Expected 150, got {_state.Balance}");
        }
    }

    // APPROACH 2: Auto-execute Givens, manual assertions
    [Scenario(SpecId = "SPEC0001", UserStory = "US01", ScenarioId = "SC02",
              Label = "Semi-auto deposit test")]
    public class WhenDepositingSemiAuto : WhenTestingFor<AccountReducer>
    {
        private AccountState _state = null!;

        protected override bool AutoExecuteGivens => true;

        [For(StateType = typeof(AccountState))]
        protected override AccountReducer For() => new AccountReducer();

        [Given(Property = "Balance", Operator = "Equals", Value = 100)]
        [Given(Property = "AccountStatus", Operator = "Equals", Value = "Active")]
        protected override void Given()
        {
            // Create the state object first
            _state = new AccountState();
            // Then AutoExecuteGivens automatically applies the attributes:
            // _state.Balance = 100
            // _state.AccountStatus = "Active"
        }

        [When(Action = "depositing funds", ActionType = "DEPOSIT")]
        protected override void When()
        {
            _state = Sut.Reduce(_state, new DepositAction(50));
        }

        [Then(Uat = "02", Comment = "Balance validation",
              Property = "Balance", Operator = "Equals", Expected = 150)]
        public void ShouldUpdateBalance()
        {
            // Manual assertion still works
            if (_state.Balance != 150)
                throw new InvalidOperationException($"Expected 150, got {_state.Balance}");
        }
    }

    // APPROACH 3: Full auto-execution
    [Scenario(SpecId = "SPEC0001", UserStory = "US01", ScenarioId = "SC03",
              Label = "Full auto deposit test")]
    public class WhenDepositingFullAuto : WhenTestingFor<AccountReducer>
    {
        private AccountState _state = null!;

        protected override bool AutoExecuteGivens => true;

        [For(StateType = typeof(AccountState))]
        protected override AccountReducer For() => new AccountReducer();

        [Given(Property = "Balance", Operator = "Equals", Value = 100)]
        [Given(Property = "AccountStatus", Operator = "Equals", Value = "Active")]
        protected override void Given()
        {
            _state = new AccountState();
            // Auto-applied by framework
        }

        [When(Action = "depositing funds", ActionType = "DEPOSIT")]
        protected override void When()
        {
            _state = Sut.Reduce(_state, new DepositAction(50));
        }

        [Then(Uat = "03", Comment = "Auto-validated balance and transaction count",
              Property = "Balance", Operator = "Equals", Expected = 150,
              PostconditionProperty = "TransactionCount", PostconditionOperator = "Equals", PostconditionExpected = 1)]
        public void ShouldUpdateBalanceAndCount()
        {
            // Auto-validate from attribute
            this.AutoValidate();
            // That's it! Framework validates:
            // - _state.Balance == 150
            // - _state.TransactionCount == 1
        }

        [Then(Uat = "04", Comment = "Multiple postconditions",
              Property = "Balance", Operator = "GreaterThan", Expected = 0,
              PostconditionProperty = "AccountStatus", PostconditionOperator = "Equals", PostconditionExpected = "Active")]
        public void ShouldMaintainActiveStatus()
        {
            this.AutoValidate();
        }
    }
}
