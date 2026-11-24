@withholdEarnings
Feature: GSL Withhold Payments for Learning

The Earnings Bridge invalidates earnings when changes require employer approval.

Scenario Outline: AC01 - Invalidate the current earnings
    Given a collection period is open for withholding
    And received '<CourseType>' earnings are stored
    And other received earnings differ by each identifying field
    And Payments receives a WithholdPaymentsForLearningEvent
    When the Earnings Events Bridge finds matching received earnings using the details on the event
    Then the matching received earnings are marked as Invalidated
    And the invalidated state is stored against the received earnings

    Examples:
        | CourseType     |
        | apprenticeship |
        | short course   |

Scenario Outline: AC02 - Ignore invalidated earnings during reprocessing
    Given a collection period is open for withholding
    And received '<CourseType>' earnings are stored
    And a withholding event has invalidated the earnings
    When the invalidated earnings are reprocessed
    Then the invalidated earnings are ignored
    And no further payments are generated from those earnings

    Examples:
        | CourseType     |
        | apprenticeship |
        | short course   |

Scenario Outline: AC03 - Publish a DAS earnings received event for invalidated earnings
    Given a collection period is open for withholding
    And received '<CourseType>' earnings are stored
    And a withholding event has invalidated the earnings
    When the Earnings Events Bridge publishes a DAS earnings received event for invalidated earnings
    Then a DasEarningsReceivedEvent is published with a temporary time-based EarningsId
    And the original received earnings remain invalidated

    Examples:
        | CourseType     |
        | apprenticeship |
        | short course   |

Scenario Outline: AC04 - Keep invalidated earnings ignored during reprocessing after new approved earnings arrive
    Given a collection period is open for withholding
    And received '<CourseType>' earnings are stored
    And a withholding event has invalidated the earnings
    When new approved earnings are received for the training
    And the invalidated earnings are reprocessed
    Then the invalidated earnings are ignored
    And no further payments are generated from those earnings

    Examples:
        | CourseType     |
        | apprenticeship |
        | short course   |
