Feature: NFR Uniquely Identify EarningEvents

EarningEvents Bridge should use unique identifiers that allow the events to be sorted in the order they were created


Scenario: Earning Events should use time ordered identifiers
	Given an employer has approved funding for a short course training
	But the earnings for the initial verion of the training delivery were not sent to the payments system
	And the employer approves funding for a change to the earnings delivery

