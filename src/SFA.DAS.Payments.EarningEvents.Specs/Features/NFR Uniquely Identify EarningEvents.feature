Feature: NFR Uniquely Identify EarningEvents

EarningEvents Bridge should use unique identifiers that allow the events to be sorted in the order they were created


Scenario: Earning Events should use time ordered identifiers
	Given an employer has already approved the initial funding a learner on an Apprenticeship Unit course
	And the earnings were persisted
	And the provider and employer have agreed a change to the delivery of training for the course within the same collection period as the previous earnings
	And the change has resulted in new earnings generated for the training
	When the Payments Earnings Bridge component receives the DAS Earnings
	Then it should convert them to a ShortCourseEarnings event
	And the earnings should use an identifier that is higher or later than the identifier used in the previous earnings
