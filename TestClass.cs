@IsTest
public class OpportunityPollingController_Test {

    // Helper to create custom setting
    private static void setupCustomSetting() {
        // Detect environment for test
        String env = (Test.isRunningTest() && !System.isProduction()) ? 'Sandbox' : 'Production';

        ThirdPartyConfig__c cfg = new ThirdPartyConfig__c(
            Environment__c = env,
            BaseUrl__c = 'https://example.com',
            MaxWaitSeconds__c = 90
        );
        insert cfg;
    }

    // Create a sample Opportunity
    private static Opportunity createOpportunity(String stage) {
        Opportunity opp = new Opportunity(
            Name = 'Test Opp',
            StageName = stage,
            CloseDate = Date.today(),
            Secret_Key__c = 'ABC123'
        );
        insert opp;
        return opp;
    }

    @IsTest
    static void testGetConfiguration() {
        setupCustomSetting();

        Test.startTest();
        Map<String, Object> config = OpportunityPollingController.getConfiguration();
        Test.stopTest();

        System.assertNotEquals(null, config, 'Config should not be null');
        System.assertEquals('https://example.com', (String)config.get('baseUrl'));
        System.assertEquals(90, (Integer)config.get('maxWaitSeconds'));
    }

    @IsTest
    static void testCheckOpportunityStage_Approved() {
        setupCustomSetting();
        Opportunity opp = createOpportunity('Approved');

        Test.startTest();
        Map<String, Object> result = OpportunityPollingController.checkOpportunityStage(opp.Id);
        Test.stopTest();

        System.assertEquals('Approved', (String)result.get('stage'));
        System.assertEquals('ABC123', (String)result.get('secretKey'));
    }

    @IsTest
    static void testCheckOpportunityStage_NotApproved() {
        setupCustomSetting();
        Opportunity opp = createOpportunity('Pending Review');

        Test.startTest();
        Map<String, Object> result = OpportunityPollingController.checkOpportunityStage(opp.Id);
        Test.stopTest();

        System.assertEquals('Pending Review', (String)result.get('stage'));
        System.assertEquals('ABC123', (String)result.get('secretKey'));
    }

    @IsTest
    static void testMissingOpportunity() {
        setupCustomSetting();

        try {
            Test.startTest();
            OpportunityPollingController.checkOpportunityStage('006XXXXXXXXXXXX'); // Fake Id
            Test.stopTest();
            System.assert(false, 'Exception should have been thrown for missing Opportunity');
        } catch (Exception e) {
            System.assert(e.getMessage().contains('List has no rows'), 'Expected no rows exception');
        }
    }

    @IsTest
    static void testMissingCustomSetting() {
        // No custom setting inserted → expect exception
        try {
            Test.startTest();
            OpportunityPollingController.getConfiguration();
            Test.stopTest();
            System.assert(false, 'Should have thrown due to missing custom setting');
        } catch (QueryException e) {
            System.assert(e.getMessage().contains('List has no rows'), 'Expected missing setting error');
        }
    }
}