import { LightningElement, api, track } from 'lwc';
import checkOpportunityStage from '@salesforce/apex/OpportunityPollingController.checkOpportunityStage';
import getConfiguration from '@salesforce/apex/OpportunityPollingController.getConfiguration';

import WAIT_IMAGES from '@salesforce/resourceUrl/WaitImages'; // folder in static resource

export default class LwcOpportunityWait extends LightningElement {

    @api opportunityId;  // input from Flow

    @track message = 'Please wait while we work on your application...';
    @track progress = 0;
    @track currentImage;

    maxSeconds = 90; // default - overwritten by config
    pollingInterval = 5000; // 5 seconds
    elapsed = 0;
    stageApproved = false;

    baseUrl;
    secretKey;

    pollTimer;
    imageList = ['img1.png','img2.png','img3.png','img4.png'];

    imageIndex = 0;


    connectedCallback() {
        this.initialize();
    }

    async initialize() {
        try {
            // Load configuration
            const config = await getConfiguration();
            this.baseUrl = config.baseUrl;
            this.maxSeconds = config.maxWaitSeconds;

            this.startWaiting();
        } catch (err) {
            this.message = 'Error loading configuration: ' + err.body?.message;
        }
    }


    startWaiting() {
        this.switchImage();
        this.pollTimer = setInterval(() => { this.pollOpportunity(); }, this.pollingInterval);
    }


    switchImage() {
        this.currentImage = `${WAIT_IMAGES}/${this.imageList[this.imageIndex]}`;
        this.imageIndex = (this.imageIndex + 1) % this.imageList.length;
    }


    async pollOpportunity() {
        this.elapsed += this.pollingInterval / 1000;
        this.progress = Math.floor((this.elapsed / this.maxSeconds) * 100);

        this.switchImage();

        try {
            const result = await checkOpportunityStage({ opportunityId: this.opportunityId });
            this.secretKey = result.secretKey;

            if (result.stage.toLowerCase() === 'approved') {
                this.stageApproved = true;
                this.handleApproved();
                return;
            }
        } catch (error) {
            this.message = 'Error checking opportunity: ' + error.body?.message;
            clearInterval(this.pollTimer);
            return;
        }

        // timeout reached
        if (this.elapsed >= this.maxSeconds) {
            clearInterval(this.pollTimer);
            this.handleTimeout();
        }
    }


    handleApproved() {
        clearInterval(this.pollTimer);
        this.message = 'Your application is approved! Redirecting shortly...';

        // Wait 5 seconds
        setTimeout(() => { this.redirectToThirdParty(); }, 5000);
    }


    redirectToThirdParty() {
        const url = `${this.baseUrl}?secret_key=${this.secretKey}`;

        // Redirect
        window.location.href = url;
    }


    handleTimeout() {
        this.message = 'We need more time to work on your application. We will get back to you.';
    }

    // Manual redirect button
    handleManualNav() {
        const url = `${this.baseUrl}?secret_key=${this.secretKey}`;
        window.open(url, '_blank');
    }
}