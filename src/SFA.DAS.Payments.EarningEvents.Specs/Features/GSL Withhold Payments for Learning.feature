@withholdEarnings
Feature: GSL Withhold Payments for Learning

The Earnings Bridge invalidates earnings when changes require employer approval.

Scenario: AC01 - Invalidate the current earnings and notify Payments
    Given there is at least one open collection period
    And previously received earnings are stored
    And non-matching earnings are stored
    When Payments receives a WithholdPaymentsForLearningEvent
    Then the matching received earnings are marked as Invalidated
    And non-matching earnings remain unaffected
    And a DasEarningsReceivedEvent is published for each open collection period
    And each event contains the same temporary EarningsId based on the current time

Scenario: AC02 - Ignore invalidated earnings
    Given there is at least one open collection period
    And previously received earnings have been marked as Invalidated
    And valid earnings are awaiting reprocessing
    When the earnings are reprocessed
    Then an earnings event is published for the valid earnings
    And the invalidated earnings remain invalidated
    And no earnings events are published for the invalidated earnings

Scenario: AC03 - Process new approved earnings
    Given there is at least one open collection period
    And the previous earnings for the training have been invalidated
    And new approved earnings are available for the training
    When the new approved earnings are received
    Then an earnings event is published for the new approved earnings
    And the invalidated earnings remain ignored

Scenario: AC04 - Withholding when there are no open collection periods
    Given there are no open collection periods
    And previously received earnings are stored
    When Payments receives a WithholdPaymentsForLearningEvent
    Then no DasEarningsReceivedEvent is published
