Feature: DAS earnings processing

  Scenario: DAS earnings are received in current collection period for payments due in future delivery period
    Given DAS Earnings Bridge receives the following earnings for the current collection period R10:
      | CollectionPeriod | DeliveryPeriod | EarningType | Amount |
      | R10              | 11             | Milestone1  | 300    |

    When the earnings are processed

    Then the earnings are written to the Earnings Cache with ProcessedOn as null
    And no further processing is carried out
    And no outbound message is published


  Scenario: DAS earnings are received in current collection period for payments due in current and future delivery periods
    Given DAS Earnings Bridge receives the following earnings for the current collection period R10:
      | CollectionPeriod | DeliveryPeriod | EarningType | Amount |
      | R10              | 10             | Milestone1  | 300    |
      | R10              | 11             | Completion  | 700    |

    When the earnings are processed

    Then the Milestone1 payment earning for the current collection period is written to the Earnings Cache with ProcessedOn populated
    And the Milestone1 payment GSO Earning Event is generated and published
    And the Milestone1 payment DAS Earnings Received Event is generated and published

    And the Completion payment earning for the current collection period is written to the Earnings Cache with ProcessedOn as null
    And the Completion payment GSO Earning Event is not generated
    And the Completion payment DAS Earnings Received Event is not generated