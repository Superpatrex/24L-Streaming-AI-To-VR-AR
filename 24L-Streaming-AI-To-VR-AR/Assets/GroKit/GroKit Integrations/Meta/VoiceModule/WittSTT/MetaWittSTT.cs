/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;
using Oculus.Voice;
using UnityEngine.Events;
using Meta.WitAi.Requests;
using Core3lb;

[HelpURL("https://developer.oculus.com/experimental/voice-sdk/tutorial-overview/")]

[Core3lbClass]
public class MetaWittSTT : AppVoiceExperience
{
    public UnityEvent onStartListening;
    public UnityEvent onStopListening;
    public UnityEvent<string> onTranscriptComplete;
    public UnityEvent<string> onPartialTranscriptGotten;

    private VoiceServiceRequest _request;

    public bool _isActive;
    [TextArea]
    public string transcripedText;
    [TextArea]
    public string partialTranscripedText;

    public void OnEnable()
    {
        VoiceEvents.OnFullTranscription.AddListener(GotAllText);
        VoiceEvents.OnPartialTranscription.AddListener(GotPartial);
    }

    public void _ToggleListening()
    {
        if(!_isActive)
        {
            _StartListening();
        }
        else
        {
            _StopListening();
        }
    }

    [CoreButton]
    public void _StartListening()
    {
        onStartListening.Invoke();
        _request = ActivateImmediately(GetRequestEvents());
    }

    [CoreButton]
    public void _StopListening()
    {
        onStopListening.Invoke();
        _request.DeactivateAudio();
    }

    public void FinishedProcessing(string text)
    {
        onTranscriptComplete.Invoke(text);
    }

    void GotAllText(string text)
    {
        transcripedText = text;
        FinishedProcessing(text);
    }

    void GotPartial(string text)
    {
        partialTranscripedText = text;
        onPartialTranscriptGotten.Invoke(text);
    }

    private VoiceServiceRequestEvents GetRequestEvents()
    {
        VoiceServiceRequestEvents events = new VoiceServiceRequestEvents();
        events.OnInit.AddListener(OnInit);
        events.OnComplete.AddListener(OnComplete);
        return events;
    }
    // Request initialized
    private void OnInit(VoiceServiceRequest request)
    {
        _isActive = true;
    }
    // Request completed
    private void OnComplete(VoiceServiceRequest request)
    {
        _isActive = false;
    }
}
