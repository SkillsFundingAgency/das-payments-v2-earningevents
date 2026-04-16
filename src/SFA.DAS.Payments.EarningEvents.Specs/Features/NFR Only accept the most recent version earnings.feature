Feature: NFR Only accept the most recent version earnings

The system should discard any earnings that are older than the most recent earnings processed.   

Scenario: Previous Earnings
Given the Payments system has already recorded the payments and associated earnings for the most recent Earnings for the training
But there was an issue in the DAS Earnings system resulting in an older set of earnings being sent to the Payments system
When the Payments Earnings Bridge component receives the older, now invalid earnings
Then it should discard the earnings


Scenario: Duplicate Earnings
Given the Payments system has already recorded the payments and associated earnings transactions for earnings that were approved today
But there was an issue in the DAS Earnings system resulting in the previous set of earnings being resent to the Payments system
When the Payments Earnings Bridge component receives the duplicate earnings
Then it should discard the earnings