Feature: PV2-4119 DAS earnings processing

Scenario: DAS earnings are received in current collection period for payments due in future delivery period
    Given that the collection period has opened for R10
    And DAS Earnings Bridge receives the following earnings for the current collection period R10:
      | CollectionPeriod | DeliveryPeriod | EarningType | Amount |
      | 10               | 11             | Milestone1  | 300    |
    When the earnings are processed

    Then the earnings are written to the Earnings Cache with ProcessedOn as null
    And no further processing is carried out
    And no outbound message is published


Scenario: DAS earnings are received in current collection period for payments due in current and future delivery periods
    Given that the collection period has opened for R10
    And DAS Earnings Bridge receives the following earnings for the current collection period R10:
      | CollectionPeriod | DeliveryPeriod | EarningType | Amount |
      | 10               | 10             | Milestone1  | 300    |
      | 10               | 11             | Completion  | 700    |

    When the earnings are processed

    Then the Milestone1 payment earning for the current collection period is written to the Earnings Cache with ProcessedOn populated
    And the Milestone1 payment GSO Earning Event is generated and published
    And the Milestone1 payment DAS Earnings Received Event is generated and published

    And the Completion payment earning for the current collection period is written to the Earnings Cache with ProcessedOn as null
    And the Completion payment GSO Earning Event is not generated