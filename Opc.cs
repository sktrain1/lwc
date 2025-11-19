public with sharing class OpportunityPollingController {

    @AuraEnabled(cacheable=false)
    public static Map<String, Object> checkOpportunityStage(Id opportunityId) {
        Opportunity opp = [
            SELECT Id, StageName, Secret_Key__c
            FROM Opportunity
            WHERE Id = :opportunityId
            LIMIT 1
        ];

        Map<String, Object> result = new Map<String, Object>();
        result.put('stage', opp.StageName);
        result.put('secretKey', opp.Secret_Key__c);

        return result;
    }


    @AuraEnabled(cacheable=true)
    public static Map<String, Object> getConfiguration() {
        // Determine environment
        String env = (System.isSandbox() ? 'Sandbox' : 'Production');

        ThirdPartyConfig__c cfg = [
            SELECT BaseUrl__c, MaxWaitSeconds__c
            FROM ThirdPartyConfig__c
            WHERE Environment__c = :env
            LIMIT 1
        ];

        Map<String, Object> result = new Map<String, Object>();
        result.put('baseUrl', cfg.BaseUrl__c);
        result.put('maxWaitSeconds', cfg.MaxWaitSeconds__c);

        return result;
    }
}