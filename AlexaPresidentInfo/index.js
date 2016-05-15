/**
    Copyright 2014-2015 Amazon.com, Inc. or its affiliates. All Rights Reserved.

    Licensed under the Apache License, Version 2.0 (the "License"). You may not use this file except in compliance with the License. A copy of the License is located at

        http://aws.amazon.com/apache2.0/

    or in the "license" file accompanying this file. This file is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

/**
 * This sample shows how to create a Lambda function for handling Alexa Skill requests that:
 *
 * - Custom slot type: demonstrates using custom slot types to handle a finite set of known values
 *
 * Examples:
 * One-shot model:
 *  User: "Alexa, ask Unit Converter to convert five degrees celsius to fahrenheit."
 *  Alexa: "()"
 */

'use strict';

var AlexaSkill = require('./AlexaSkill');

var APP_ID = "amzn1.echo-sdk-ams.app.5a6a551a-550e-4a27-806e-f67e90b1c243"; //replace with 'amzn1.echo-sdk-ams.app.[your-unique-value-here]';

/**
 * UnitConverter is a child of AlexaSkill.
 * To read more about inheritance in JavaScript, see the link below.
 *
 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Introduction_to_Object-Oriented_JavaScript#Inheritance
 */
var UnitConverter = function () {
    AlexaSkill.call(this, APP_ID);
};

// Extend AlexaSkill
UnitConverter.prototype = Object.create(AlexaSkill.prototype);
UnitConverter.prototype.constructor = UnitConverter;

UnitConverter.prototype.eventHandlers.onLaunch = function (launchRequest, session, response) {
    var speechText = "Welcome to the Unit Converter. You can ask me to convert between various measurements, like temperature or length.";
    // If the user either does not reply to the welcome message or says something that is not
    // understood, they will be prompted again with this text.
    var repromptText = "For instructions on what you can say, please say help me.";
    response.ask(speechText, repromptText);
};

UnitConverter.prototype.intentHandlers = {
    "ConvertTemp": function (intent, session, response) {
        var tempValueSlot = intent.slots.TempValue;
        var tempUnitInSlot = intent.slots.TempUnitIn;
        var tempUnitOutSlot = intent.slots.TempUnitOut;
       
        var cardTitle = "Conversion for " + tempValueSlot.value + " degrees " + tempUnitInSlot.value,
            convertedValue = "five degrees " + tempUnitOutSlot.value, // TODO: dummy value
            speechOutput,
            repromptOutput;
        if (convertedValue) {
            speechOutput = {
                speech: convertedValue,
                type: AlexaSkill.speechOutputType.PLAIN_TEXT
            };
            response.tellWithCard(speechOutput, cardTitle, convertedValue);
        } else {
            var speech = "Does not compute. Oops. What else can I help with?";
            speechOutput = {
                speech: speech,
                type: AlexaSkill.speechOutputType.PLAIN_TEXT
            };
            repromptOutput = {
                speech: "What else can I help with?",
                type: AlexaSkill.speechOutputType.PLAIN_TEXT
            };
            response.ask(speechOutput, repromptOutput);
        }
    },

    "AMAZON.StopIntent": function (intent, session, response) {
        var speechOutput = "Goodbye";
        response.tell(speechOutput);
    },

    "AMAZON.CancelIntent": function (intent, session, response) {
        var speechOutput = "Goodbye";
        response.tell(speechOutput);
    },

    "AMAZON.HelpIntent": function (intent, session, response) {
        var speechText = "I'm not very smart yet. Now, what can I help you with?";
        var repromptText = "You can ask for temperature conversion. Now, what can I help you with?";
        var speechOutput = {
            speech: speechText,
            type: AlexaSkill.speechOutputType.PLAIN_TEXT
        };
        var repromptOutput = {
            speech: repromptText,
            type: AlexaSkill.speechOutputType.PLAIN_TEXT
        };
        response.ask(speechOutput, repromptOutput);
    }
};

exports.handler = function (event, context) {
    var unitConverter = new UnitConverter();
    unitConverter.execute(event, context);
};
