@AssetLifecycle
Feature: Asset Management Full Lifecycle (SC-002)
    Full lifecycle on the empty register: group setup, asset register,
    transfer, depreciation, disposal, revaluation, impairment, and physical count.

Background:
    Given a logged in user with asset management permissions

Scenario: 1. Create asset group with defaults and attribute bindings
    Given the user navigates to asset groups
    When the user creates a group with code "GRP-001" and name "معدات مكتبية"
    And the user sets depreciation method to "StraightLine" with rate 20
    And the user sets the depreciation account
    And the user binds attribute "اللون" (text, required)
    And the user binds attribute "الوزن" (decimal, optional)
    Then the group is saved and appears in the list

Scenario: 2. Register an asset with employee custodian and attributes
    Given the user navigates to asset register
    When the user creates an asset under group "GRP-001"
    And the user sets employee custodian
    And the user enters attribute value "اللون" = "أبيض"
    And the user activates the asset
    Then the asset status is "Active"
    And the derived department matches the custodian's department

Scenario: 3. Execute a transfer
    Given an active asset exists
    And the user navigates to transfers
    When the user creates a transfer for the asset
    And the user sets new location and custodian
    And the user executes the transfer
    Then the asset card reflects the destination
    And the transfer history preserves the source

Scenario: 4. Run and post depreciation
    Given an active depreciable asset exists
    And the user navigates to depreciation
    When the user runs depreciation for the current period
    Then depreciation schedules are created
    When the user posts the depreciation
    Then the journal entry is balanced
    And the asset accumulated depreciation is updated
    And the schedule status is "Posted"

Scenario: 5. Reverse a posted depreciation
    Given a posted depreciation schedule exists
    When the user reverses the schedule with reason "تصحيح"
    Then a reversal record is linked to the original
    And the original record is untouched
    And the reversal has its own journal entry

Scenario: 6. Dispose of an asset
    Given another active asset exists
    And the user navigates to disposals
    When the user creates a disposal with method "بيع"
    And the user enters sale proceeds
    And the user posts the disposal
    Then the asset status is "Disposed"
    And gain or loss is derived
    And the journal entry is linked

Scenario: 7. Record revaluation and impairment with reversal
    Given an active asset with book value exists
    And the user navigates to revaluations
    When the user creates a revaluation with new value higher than book value
    And the user posts the revaluation
    Then the asset value is updated
    And the revaluation direction is "increase"
    Given the user navigates to impairments
    When the user creates an impairment with recoverable amount
    And the user posts the impairment
    Then the impairment is recorded
    When the user reverses the impairment with reason "استرداد"
    Then the reversal is linked to the original impairment

Scenario: 8. Conduct entity-wide physical count
    Given assets exist in the register
    And the user navigates to counts
    When the user creates an entity-wide count
    Then the scope label shows "جميع المواقع — جميع الإدارات"
    When the user starts the count
    Then system fields are frozen
    And observation lines are generated
    When the user records observations including one not-found
    Then completion is blocked while lines are "not examined"
    When the user sets all lines to definite states
    And the user completes the count
    And the user reviews the count
    Then the count status is "Reviewed"
