Feature: Payments Are Paused For Learner

EarningEvents Bridge should indicate to downstream services whether payments for a learner are paused 

Background: 
	Given the collection period has opened recently

Scenario: Earning Events should indicate if payments are paused for a learner
	Given we receive a message indicating that payments are paused for the learner
	When new changes are approved and the resultant earnings are sent to the Payments system
	Then the earnings should have the IsPaymentPaused property set to true
