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

var AlexaSkill = require('./AlexaSkill'),
    PresidentData = require('./constants.js');

var APP_ID = "amzn1.echo-sdk-ams.app.5a6a551a-550e-4a27-806e-f67e90b1c243"; //replace with 'amzn1.echo-sdk-ams.app.[your-unique-value-here]';

/**
 * PresidentInfo is a child of AlexaSkill.
 * To read more about inheritance in JavaScript, see the link below.
 *
 * @see https://developer.mozilla.org/en-US/docs/Web/JavaScript/Introduction_to_Object-Oriented_JavaScript#Inheritance
 */
var PresidentInfo = function () {
    AlexaSkill.call(this, APP_ID);
};

// Extend AlexaSkill
PresidentInfo.prototype = Object.create(AlexaSkill.prototype);
PresidentInfo.prototype.constructor = PresidentInfo;

PresidentInfo.prototype.eventHandlers.onLaunch = function (launchRequest, session, response) {
    var speechText = "I'll tell you key information about the Presidents of the United States.";
    // If the user either does not reply to the welcome message or says something that is not
    // understood, they will be prompted again with this text.
    var repromptText = "For instructions on what you can say, please say help me.";
    response.ask(speechText, repromptText);
};

PresidentInfo.prototype.intentHandlers = {
    "PresidentInfoIntent": function (intent, session, response) {
        var ordinalSlot = intent.slots.OrdinalValue;
        var numeralSlot = intent.slots.NumeralValue;
        var nameSlot = intent.slots.NameValue;
        var president;
        
        // Note that the user's ordinal query is one-indexed.
        if (numeralSlot && numeralSlot.value >= 1 && numeralSlot.value < PresidentData.length + 1)
        {
            president = PresidentData[numeralSlot.value - 1];
        }
        else if (ordinalSlot && ordinalSlot.value)
        {
            //console.log("Ordinal: " + ordinalSlot.value);
            for (var i = 0; i < PresidentData.length; i++) {
                //console.log("Searching " + i + ": " + PresidentData[i].Ordinal);
                if ((PresidentData[i].Ordinal == ordinalSlot.value) ||
                // Alexa seems to send inconsistent ordinals, e.g. "second" vs. "3rd"
                    (PresidentData[i].Ordinal2 == ordinalSlot.value)) {
                    president = PresidentData[i];
                }
            }
        }
        else if (nameSlot && nameSlot.value)
        {
            nameSlot.value = nameSlot.value.toLowerCase();
            
            for (var i = 0; i < PresidentData.length; i++) {
                if (PresidentData[i].MatchName == nameSlot.value) {
                    president = PresidentData[i];
                }
            }
        }
        
        if (!president) {
            var speech = "Does not compute. Oops. What else can I help with?";
            speechOutput = {
                speech: speech,
                type: AlexaSkill.speechOutputType.PLAIN_TEXT
            };
            var repromptOutput = {
                speech: "What else can I help with?",
                type: AlexaSkill.speechOutputType.PLAIN_TEXT
            };
            response.ask(speechOutput, repromptOutput);
            
            return;
        }
       
        var cardTitle = "President # " + president.Number + ", " + president.Name;
        var output = "President number " + president.Number + ", " + president.Name + ", was inaugurated on " +
                president.DateInaugurated + " and was a member of the " + president.Party + " party.";
            
        var speechOutput = {
            speech: output,
            type: AlexaSkill.speechOutputType.PLAIN_TEXT
        };
        response.tellWithCard(speechOutput, cardTitle, output);
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
        var speechText = "Ask for a United States President by their name or number. Now, what can I help you with?";
        var repromptText = speechText;
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
    var presidentInfo = new PresidentInfo();
    presidentInfo.execute(event, context);
};
