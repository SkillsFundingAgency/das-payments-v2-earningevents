@fm36HiringPayments
Feature: FM36 Hiring Payments

PV2-4329
As the Payments Service
I want to include stored employer hiring payments when processing a non-levy FM36 learner
So that the hiring payments are paid to the provider alongside the learner's normal earnings

Scenario: Eligible non-levy learner with stored hiring payments has them included in the ACT1 event
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type                | Amount |
		| 3               | FirstEmployerHiringPayment  | 500    |
		| 3               | SecondEmployerHiringPayment | 250    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event includes the learner's on-programme earnings
	And the ACT1 Earnings Event includes the stored hiring payments
		| Delivery Period | Earning Type                | Amount |
		| 3               | FirstEmployerHiringPayment  | 500    |
		| 3               | SecondEmployerHiringPayment | 250    |
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Eligible non-levy learner without stored hiring payments is published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And no hiring payment earnings are stored for the learner
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Eligible non-levy learner with stored earnings that contain no hiring payments is published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type | Amount |
		| 3               | Learning     | 400    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Learner aged 25 or over is published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 25 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type               | Amount |
		| 3               | FirstEmployerHiringPayment | 500    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Learner starting before 1 October 2026 is published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-09-30 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type               | Amount |
		| 3               | FirstEmployerHiringPayment | 500    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Learner with a levy employer is published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a Levy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type               | Amount |
		| 3               | FirstEmployerHiringPayment | 500    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner

Scenario: Hiring payments for future delivery periods are not included in the ACT1 event
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type                | Amount |
		| 3               | FirstEmployerHiringPayment  | 500    |
		| 5               | SecondEmployerHiringPayment | 250    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event includes the stored hiring payments
		| Delivery Period | Earning Type               | Amount |
		| 3               | FirstEmployerHiringPayment | 500    |
	And the ACT1 Earnings Event does not include hiring payments for delivery period 5

Scenario: Hiring payments for past delivery periods are included in the ACT1 event
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type                | Amount |
		| 2               | FirstEmployerHiringPayment  | 500    |
		| 3               | SecondEmployerHiringPayment | 250    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event includes the stored hiring payments
		| Delivery Period | Earning Type                | Amount |
		| 2               | FirstEmployerHiringPayment  | 500    |
		| 3               | SecondEmployerHiringPayment | 250    |

Scenario: Stored earnings for a short course are published as an ACT1 event with no hiring payments
	Given an FM36 apprenticeship learner who started on 2026-10-01 aged 18 at the start of learning
	And the stored earnings are for a NonLevy employer
	And the stored earnings have the course type ShortCourse
	And the following hiring payment earnings are stored for the learner
		| Delivery Period | Earning Type               | Amount |
		| 3               | FirstEmployerHiringPayment | 500    |
	When the learner is processed by Earning Events
	Then an ACT1 Earnings Event is published for the learner
	And the ACT1 Earnings Event does not include any hiring payments
	And no GSL Apprenticeship Earnings Event is published for the learner
