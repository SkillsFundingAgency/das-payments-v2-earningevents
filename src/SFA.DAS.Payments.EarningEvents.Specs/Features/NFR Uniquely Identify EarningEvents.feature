Feature: NFR Uniquely Identify EarningEvents

EarningEvents Bridge should use unique identifiers that allow the events to be sorted in the order they were created

Background: 
	Given the collection period has opened recently

Scenario: Earning Events should use time ordered identifiers
	Given a previous set of earnings were recorded for the short course
	When new changes are approved and the resultant earnings are sent to the Payments system
	Then the new earnings should have identifiers that indicate they are later than the previous earnings
