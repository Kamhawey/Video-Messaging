let mediaRecorder = null;
let mediaStream = null;
let recordedChunks = [];
let recordedVideoUrl = null;
let isPaused = false;

// Check if browser supports media recording
export function checkMediaRecordingSupport() {
    return !!(navigator.mediaDevices &&
        navigator.mediaDevices.getUserMedia &&
        window.MediaRecorder);
}

// Check MP4 support specifically
export function checkMP4RecordingSupport() {
    const mp4Types = [
        'video/mp4',
        'video/mp4;codecs=avc1.42E01E,mp4a.40.2',
        'video/mp4;codecs=avc1.42E01E',
        'video/mp4;codecs=mp4a.40.2'
    ];

    return mp4Types.some(type => MediaRecorder.isTypeSupported(type));
}

// Get best supported MIME type with MP4 priority
function getBestMimeType() {
    // Priority order: MP4 variants first, then WebM as fallback
    const mimeTypes = [
        'video/mp4',
        'video/mp4;codecs=avc1.42E01E,mp4a.40.2',
        'video/mp4;codecs=avc1.42E01E',
        'video/mp4;codecs=mp4a.40.2',
        'video/webm;codecs=vp9,opus',
        'video/webm;codecs=vp8,opus',
        'video/webm'
    ];

    for (const mimeType of mimeTypes) {
        if (MediaRecorder.isTypeSupported(mimeType)) {
            console.log(`Selected MIME type: ${mimeType}`);
            return mimeType;
        }
    }

    // Fallback to default if none supported (shouldn't happen in modern browsers)
    return 'video/webm';
}

// Initialize video element
export function initializeVideoElement(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return false;
        }

        // Set up video element properties
        videoElement.muted = (elementId === 'livePreview'); // Mute live preview to avoid feedback
        videoElement.playsInline = true;
        videoElement.preload = 'metadata'; // Ensure metadata is loaded

        // Add event listeners for better metadata handling
        videoElement.addEventListener('loadedmetadata', () => {
            console.log(`Metadata loaded for ${elementId}, duration: ${videoElement.duration}`);
        });
        return true;
    } catch (error) {
        console.error('Error initializing video element:', error);
        return false;
    }
}

// Capture thumbnail from video element
export function captureThumbnail(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return '';
        }

        // Create a canvas to capture the current frame
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');

        // Set canvas dimensions to match video
        canvas.width = videoElement.videoWidth || videoElement.clientWidth;
        canvas.height = videoElement.videoHeight || videoElement.clientHeight;

        // Draw the current video frame to canvas
        context.drawImage(videoElement, 0, 0, canvas.width, canvas.height);

        // Convert canvas to data URL (base64 image)
        const thumbnailUrl = canvas.toDataURL('image/jpeg', 0.8);

        console.log('Thumbnail captured successfully');
        return thumbnailUrl;
    } catch (error) {
        console.error('Error capturing thumbnail:', error);
        return '';
    }
}

// Start media recording
export async function startMediaRecording() {
    try {
        // Reset previous recording
        recordedChunks = [];
        recordedVideoUrl = null;
        isPaused = false;

        // Get user media (camera and microphone)
        mediaStream = await navigator.mediaDevices.getUserMedia({
            video: {
                width: { ideal: 1280 },
                height: { ideal: 720 },
                frameRate: { ideal: 30 }
            },
            audio: {
                echoCancellation: true,
                noiseSuppression: true,
                autoGainControl: true
            }
        });

        // Set up live preview
        const livePreview = document.getElementById('livePreview');
        if (livePreview) {
            livePreview.srcObject = mediaStream;
        }

        // Get the best supported MIME type (prioritizing MP4)
        const mimeType = getBestMimeType();

        // Create MediaRecorder with optimized settings
        const options = {
            mimeType: mimeType
        };

        // Add bitrate settings if supported
        if (mimeType.includes('mp4')) {
            // MP4 specific settings
            options.videoBitsPerSecond = 2500000; // 2.5 Mbps
            options.audioBitsPerSecond = 128000;   // 128 kbps
        } else {
            // WebM fallback settings
            options.videoBitsPerSecond = 2500000;
            options.audioBitsPerSecond = 128000;
        }

        mediaRecorder = new MediaRecorder(mediaStream, options);

        // Handle data available event
        mediaRecorder.ondataavailable = (event) => {
            if (event.data && event.data.size > 0) {
                recordedChunks.push(event.data);
            }
        };

        // Handle recording stop event
        mediaRecorder.onstop = () => {
            const blob = new Blob(recordedChunks, { type: mimeType });
            recordedVideoUrl = URL.createObjectURL(blob);

            // Log the final format
            console.log(`Recording completed in format: ${mimeType}`);
            console.log(`Blob size: ${blob.size} bytes`);

            // Clear live preview
            const livePreview = document.getElementById('livePreview');
            if (livePreview) {
                livePreview.srcObject = null;
            }
        };

        // Start recording
        mediaRecorder.start(1000); // Collect data every second

        return true;
    } catch (error) {
        console.error('Error starting media recording:', error);
        return false;
    }
}

// Convert WebM to MP4 (client-side conversion using FFmpeg.js - optional)
export async function convertToMP4(webmBlob) {
    try {
        // This is a placeholder for client-side conversion
        // You would need to implement FFmpeg.js or similar for actual conversion
        console.log('Client-side conversion not implemented');
        return webmBlob;
    } catch (error) {
        console.error('Error converting to MP4:', error);
        return webmBlob;
    }
}

// Get recorded video info
export function getRecordedVideoInfo() {
    if (!recordedVideoUrl) return null;

    return {
        url: recordedVideoUrl,
        format: mediaRecorder ? mediaRecorder.mimeType : 'unknown',
        size: recordedChunks.reduce((total, chunk) => total + chunk.size, 0)
    };
}

// Pause media recording
export function pauseMediaRecording() {
    try {
        if (mediaRecorder && mediaRecorder.state === 'recording') {
            mediaRecorder.pause();
            isPaused = true;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error pausing media recording:', error);
        return false;
    }
}

// Resume media recording
export function resumeMediaRecording() {
    try {
        if (mediaRecorder && mediaRecorder.state === 'paused') {
            mediaRecorder.resume();
            isPaused = false;
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error resuming media recording:', error);
        return false;
    }
}

// Stop media recording
export function stopMediaRecording() {
    return new Promise((resolve, reject) => {
        try {
            if (!mediaRecorder || mediaRecorder.state === 'inactive') {
                resolve(true);
                return;
            }

            // Set up the stop handler before stopping
            mediaRecorder.onstop = () => {
                const blob = new Blob(recordedChunks, { type: mediaRecorder.mimeType });
                recordedVideoUrl = URL.createObjectURL(blob);

                console.log('Recording stopped, video URL created:', recordedVideoUrl);
                console.log('Final format:', mediaRecorder.mimeType);

                // Clear live preview
                const livePreview = document.getElementById('livePreview');
                if (livePreview) {
                    livePreview.srcObject = null;
                }

                // Stop all media tracks
                if (mediaStream) {
                    mediaStream.getTracks().forEach(track => {
                        track.stop();
                    });
                    mediaStream = null;
                }

                isPaused = false;
                resolve(true);
            };

            mediaRecorder.onerror = (error) => {
                console.error('MediaRecorder error:', error);
                reject(error);
            };

            mediaRecorder.stop();
        } catch (error) {
            console.error('Error stopping media recording:', error);
            reject(error);
        }
    });
}

// Get recorded video URL
export function getRecordedVideoUrl() {
    return recordedVideoUrl || '';
}

// Get recording state
export function getRecordingState() {
    if (!mediaRecorder) return 'inactive';
    return mediaRecorder.state;
}

// Check if recording is paused
export function isRecordingPaused() {
    return isPaused;
}

// Play video
export function playVideo(elementId, videoUrl) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return false;
        }

        videoElement.src = videoUrl;
        return videoElement.play()
            .then(() => true)
            .catch((error) => {
                console.error('Error playing video:', error);
                return false;
            });
    } catch (error) {
        console.error('Error in playVideo:', error);
        return false;
    }
}

// Pause video
export function pauseVideo(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return false;
        }

        videoElement.pause();
        return true;
    } catch (error) {
        console.error('Error pausing video:', error);
        return false;
    }
}

export function resumeVideo(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (videoElement) {
            videoElement.play();
            return true;
        }
        return false;
    } catch (error) {
        console.error('Error resuming video:', error);
        return false;
    }
}

// Get video current time
export function getVideoCurrentTime(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return 0;
        }

        return videoElement.currentTime;
    } catch (error) {
        console.error('Error getting video current time:', error);
        return 0;
    }
}

// Get video duration
export function getVideoDuration(elementId) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return 0;
        }

        return videoElement.duration || 0;
    } catch (error) {
        console.error('Error getting video duration:', error);
        return 0;
    }
}

// Set video current time
export function setVideoCurrentTime(elementId, currentTime) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return false;
        }

        videoElement.currentTime = currentTime;
        return true;
    } catch (error) {
        console.error('Error setting video current time:', error);
        return false;
    }
}

// Set video playback rate
export function setVideoPlaybackRate(elementId, playbackRate) {
    try {
        const videoElement = document.getElementById(elementId);
        if (!videoElement) {
            console.error(`Video element with id ${elementId} not found`);
            return false;
        }

        videoElement.playbackRate = playbackRate;
        return true;
    } catch (error) {
        console.error('Error setting video playback rate:', error);
        return false;
    }
}

// Clean up resources
export function cleanup() {
    try {
        // Stop recording if active
        if (mediaRecorder && mediaRecorder.state !== 'inactive') {
            mediaRecorder.stop();
        }

        // Stop media stream
        if (mediaStream) {
            mediaStream.getTracks().forEach(track => {
                track.stop();
            });
            mediaStream = null;
        }

        // Clear live preview
        const livePreview = document.getElementById('livePreview');
        if (livePreview) {
            livePreview.srcObject = null;
        }

        // Clean up recorded video URL
        if (recordedVideoUrl) {
            URL.revokeObjectURL(recordedVideoUrl);
            recordedVideoUrl = null;
        }

        // Clear recorded chunks
        recordedChunks = [];
        isPaused = false;

        return true;
    } catch (error) {
        console.error('Error during cleanup:', error);
        return false;
    }
}
export function getElementBoundingRect(elementId) {
    try {
        const element = document.getElementById(elementId);
        if (!element) {
            console.error(`Element with id ${elementId} not found`);
            return { left: 0, top: 0, width: 0, height: 0 };
        }

        const rect = element.getBoundingClientRect();
        return {
            left: rect.left,
            top: rect.top,
            width: rect.width,
            height: rect.height
        };
    } catch (error) {
        console.error('Error getting element bounding rect:', error);
        return { left: 0, top: 0, width: 0, height: 0 };
    }
}


// Get video blob data for IFormFile conversion
export async function getVideoBlob() {
    try {
        if (!recordedVideoUrl) {
            console.error('No recorded video URL available');
            return null;
        }

        // Fetch the blob from the object URL
        const response = await fetch(recordedVideoUrl);
        const blob = await response.blob();

        // Get the MIME type from the MediaRecorder
        const mimeType = mediaRecorder ? mediaRecorder.mimeType : 'video/webm';

        // Convert blob to array buffer
        const arrayBuffer = await blob.arrayBuffer();
        const uint8Array = new Uint8Array(arrayBuffer);

        // Generate filename based on MIME type
        const extension = mimeType.includes('mp4') ? 'mp4' : 'webm';
        const filename = `recorded_video_${Date.now()}.${extension}`;

        return {
            data: Array.from(uint8Array), // Convert to regular array for serialization
            mimeType: mimeType,
            filename: filename,
            size: blob.size
        };
    } catch (error) {
        console.error('Error getting video blob:', error);
        return null;
    }
}
export async function getVideoAsDataUrl() {
    try {
        if (!recordedVideoUrl) {
            console.error('No recorded video URL available');
            return null;
        }

        const response = await fetch(recordedVideoUrl);
        const blob = await response.blob();

        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onloadend = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(blob);
        });
    } catch (error) {
        console.error('Error getting video as data URL:', error);
        return null;
    }
}

// Download video file (for testing purposes)
export function downloadVideoFile() {
    try {
        if (!recordedVideoUrl) {
            console.error('No recorded video URL available');
            return false;
        }

        const mimeType = mediaRecorder ? mediaRecorder.mimeType : 'video/webm';
        const extension = mimeType.includes('mp4') ? 'mp4' : 'webm';
        const filename = `recorded_video_${Date.now()}.${extension}`;

        // Create download link
        const downloadLink = document.createElement('a');
        downloadLink.href = recordedVideoUrl;
        downloadLink.download = filename;
        downloadLink.style.display = 'none';

        document.body.appendChild(downloadLink);
        downloadLink.click();
        document.body.removeChild(downloadLink);

        console.log(`Downloaded: ${filename}`);
        return true;
    } catch (error) {
        console.error('Error downloading video file:', error);
        return false;
    }
}

// Listen for beforeunload to cleanup resources
window.addEventListener('beforeunload', cleanup);