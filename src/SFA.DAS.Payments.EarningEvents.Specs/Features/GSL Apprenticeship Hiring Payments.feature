@gslApprenticeship
Feature: GSL Apprenticeship Hiring Payments

PV2-4330
As the Payments Service
I want to process incoming AS earnings for the employer hiring payment
So that the relevant payments are paid to the providers

Scenario: Receive apprenticeship earnings including the first employer hiring payment
	Given the payments service receives a CalculateGrowthAndSkillsPayments event for apprenticeship payments
	And the event contains the following earnings
		| Academic Year | Delivery Period | Earning Type               | Amount |
		| 2526          | 1               | Learning                   | 1000   |
		| 2526          | 1               | FirstEmployerHiringPayment | 500    |
	And the following collection periods are open
		| Collection Period | Academic Year |
		| R01               | 2526          |
	When the event is processed
	Then the following outgoing GSL Apprenticeship Earnings Event is published
		| Collection Period | Academic Year | Delivery Period | Transaction Type | Amount |
		| R01               | 2526          | 1               | Learning         | 1000   |
	And the outgoing GSL Apprenticeship Earnings Event includes the following hiring payments
		| Collection Period | Academic Year | Delivery Period | Transaction Type           | Amount |
		| R01               | 2526          | 1               | FirstEmployerHiringPayment | 500    |
	And the outgoing DAS Earnings Received event is published for collection period R01 of academic year 2526
	And all learner details, training details and payment details match the values on the incoming message
	And the incoming earnings have been saved in the Earnings Bridge cache tables

Scenario: Receive apprenticeship earnings including the second employer hiring payment
	Given the payments service receives a CalculateGrowthAndSkillsPayments event for apprenticeship payments
	And the event contains the following earnings
		| Academic Year | Delivery Period | Earning Type                | Amount |
		| 2526          | 3               | Learning                    | 1000   |
		| 2526          | 3               | SecondEmployerHiringPayment | 500    |
	And the following collection periods are open
		| Collection Period | Academic Year |
		| R03               | 2526          |
	When the event is processed
	Then the following outgoing GSL Apprenticeship Earnings Event is published
		| Collection Period | Academic Year | Delivery Period | Transaction Type | Amount |
		| R03               | 2526          | 3               | Learning         | 1000   |
	And the outgoing GSL Apprenticeship Earnings Event includes the following hiring payments
		| Collection Period | Academic Year | Delivery Period | Transaction Type            | Amount |
		| R03               | 2526          | 3               | SecondEmployerHiringPayment | 500    |
	And the outgoing DAS Earnings Received event is published for collection period R03 of academic year 2526
	And all learner details, training details and payment details match the values on the incoming message
	And the incoming earnings have been saved in the Earnings Bridge cache tables

Scenario: Receive earnings for the hiring payments only
	Given the payments service receives a CalculateGrowthAndSkillsPayments event for apprenticeship payments
	And the event contains the following earnings
		| Academic Year | Delivery Period | Earning Type                | Amount |
		| 2526          | 1               | FirstEmployerHiringPayment  | 500    |
		| 2526          | 3               | SecondEmployerHiringPayment | 500    |
	And the following collection periods are open
		| Collection Period | Academic Year |
		| R03               | 2526          |
	When the event is processed
	Then the incoming earnings have been saved in the Earnings Bridge cache tables awaiting processing
	And no outgoing GSL Apprenticeship Earnings Event is published
	And no outgoing DAS Earnings Received event is published
