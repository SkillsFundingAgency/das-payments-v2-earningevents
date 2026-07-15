Feature: PV2-4162 - Apply 100% SFA Funding for Levy Employers with Insufficient/Zero Funds and Learners aged under 25

Scenario: Levy employer with insufficient balance - Fully funded from co-investment - Start date on or after 1st August - Learner aged under 25 (happy path)

Given a message is received for a Levy employer with a GSO learner
And the Employer has insufficient funds
And the learning start date is on or after 1 August 2026
And the transaction type is a <transactionType> payment
And the learner is aged under 25 on the start date
When the payments are generated
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Milestone1      |
| Completion      |

Scenario: Levy employer with insufficient funds - Funded from co-investment - Start date before 1st August - Learner under 25 yrs(regression)

Given a CalculatedRequiredLevyAmount message is received for a Levy employer with a GSO learner
And the Employer has insufficient funds
And the learning start date is before 1 August 2026
And the transaction type is a <transactionType> payment
And the learner is aged under 25 on the start date
When the payments are generated
Then the payment funding is split between 'SFA co-investment' (95%) and 'Employer co-investment' (5%)

Examples:
| transactionType |
| Milestone1      |
| Completion      |


Scenario: Change of Levy Employer to Non-Levy Employer  - Start date on after August 1 2026  )

Given a Learner changes from a Levy to a Non-levy employer
And the learning start date is on or after 1 August 2026
And the learner is aged under 25 on the start date
And the transaction type is a <transactionType> payment
When the payments are generated
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Milestone1      |
| Completion      |


Scenario: Change from Non- Levy Employer to Levy Employer  - Start date on /after August 1 2026  )

Given a Learner changes from a Non-Levy to a Levy employer
And the learning start date is on or after 1 August 2026
And the learner is aged under 25 on the start date
And the transaction type is a <transactionType> payment
When the payments are generated
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Milestone1      |
| Completion      |