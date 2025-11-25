import cv2
import dlib
import numpy as np
from scipy.spatial import distance as dist
from collections import deque
import time
import socket

# --- Networking Setup ---
UDP_IP = "127.0.0.1"  # Localhost
UDP_PORT = 5005       # Port to send to (must match Unity)
sock = socket.socket(socket.AF_INET,
                      socket.SOCK_DGRAM) # UDP

# --- Constants ---
# Threshold for EAR to register a blink
EYE_AR_THRESH = 0.255
# Number of consecutive frames the EAR must be below the threshold
EYE_AR_CONSEC_FRAMES = 1
# Maximum frames for a valid blink (prevents long closures from being counted)
MAX_BLINK_FRAMES = 15
# Minimum face area to consider detection valid
MIN_FACE_AREA = 8000
# EAR smoothing window size
EAR_SMOOTHING_WINDOW = 2

# --- Globals ---
# Frame counter for consecutive frames with low EAR
FRAME_COUNTER = 0
# Total blink count
TOTAL_BLINKS = 0
# EAR history for smoothing
ear_history = deque(maxlen=EAR_SMOOTHING_WINDOW)
# Track last blink time to prevent double counting
last_blink_time = 0
BLINK_COOLDOWN = 0.1  # 20q0ms cooldown between blinks

def eye_aspect_ratio(eye):
    """Calculates the Eye Aspect Ratio (EAR) for a single eye."""
    # Compute the euclidean distances between the two sets of
    # vertical eye landmarks (x, y)-coordinates
    A = dist.euclidean(eye[1], eye[5])
    B = dist.euclidean(eye[2], eye[4])
    # Compute the euclidean distance between the horizontal
    # eye landmark (x, y)-coordinates
    C = dist.euclidean(eye[0], eye[3])
    # Compute the eye aspect ratio
    ear = (A + B) / (2.0 * C)
    return ear

def is_face_frontal(landmarks):
    """Check if face is roughly frontal by analyzing landmark positions."""
    # Get nose tip and face center points
    nose_tip = landmarks[30]
    left_face = landmarks[0]
    right_face = landmarks[16]
    
    # Calculate face width and nose position relative to face center
    face_width = right_face[0] - left_face[0]
    face_center_x = (left_face[0] + right_face[0]) / 2
    nose_offset = abs(nose_tip[0] - face_center_x)
    
    # If nose is too far from center, face is likely turned away
    return nose_offset < face_width * 0.15

def get_face_area(rect):
    """Calculate the area of the detected face rectangle."""
    return rect.width() * rect.height()

def smooth_ear(ear_value, history):
    """Apply smoothing to EAR values to reduce noise."""
    history.append(ear_value)
    return sum(history) / len(history)

# --- Main Setup ---
print("INFO: Loading facial landmark predictor...")
# Initialize dlib's face detector (HOG-based)
detector = dlib.get_frontal_face_detector()

try:
    # Create the facial landmark predictor
    predictor = dlib.shape_predictor("shape_predictor_68_face_landmarks.dat")
except:
    print("ERROR: Could not load shape_predictor_68_face_landmarks.dat")
    print("Please download it from: https://github.com/davisking/dlib-models")
    exit()

# Get the indices of the facial landmarks for the left and right eye
(lStart, lEnd) = (42, 48)
(rStart, rEnd) = (36, 42)

print("INFO: Starting video stream...")
# Start the video stream from the webcam
cap = cv2.VideoCapture(0)

# Check if camera opened successfully
if not cap.isOpened():
    print("ERROR: Could not open camera")
    exit()

# Set camera properties for better performance
cap.set(cv2.CAP_PROP_FRAME_WIDTH, 1280)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, 720)
cap.set(cv2.CAP_PROP_FPS, 60)

print("INFO: Press 'r' to reset blink counter, 'q' to quit")

# --- Main Loop ---
while True:
    # Grab the frame from the threaded video file stream
    ret, frame = cap.read()
    if not ret:
        print("ERROR: Failed to grab frame")
        break
    
    # Flip frame horizontally for mirror effect
    frame = cv2.flip(frame, 1)
    gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
    
    # Detect faces in the grayscale frame
    rects = detector(gray, 0)
    
    # Variables for this frame
    valid_face_detected = False
    current_ear = 0
    
    # Loop over the face detections
    for rect in rects:
        # Check if face is large enough (user is close enough to camera)
        face_area = get_face_area(rect)
        if face_area < MIN_FACE_AREA:
            continue
            
        # Determine the facial landmarks for the face region
        shape = predictor(gray, rect)
        shape = np.array([(shape.part(i).x, shape.part(i).y) for i in range(68)])
        
        # Check if face is roughly frontal
        if not is_face_frontal(shape):
            cv2.putText(frame, "Turn face towards camera", (10, 100),
                       cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 165, 255), 2)
            continue
        
        valid_face_detected = True
        
        # Extract the left and right eye coordinates
        leftEye = shape[lStart:lEnd]
        rightEye = shape[rStart:rEnd]
        
        # Calculate the EAR for both eyes
        leftEAR = eye_aspect_ratio(leftEye)
        rightEAR = eye_aspect_ratio(rightEye)
        
        # Average the EAR together for both eyes
        raw_ear = (leftEAR + rightEAR) / 2.0
        
        # Apply smoothing to reduce noise
        current_ear = smooth_ear(raw_ear, ear_history)
        
        # Draw eye contours for visualization
        cv2.drawContours(frame, [cv2.convexHull(leftEye)], -1, (0, 255, 0), 1)
        cv2.drawContours(frame, [cv2.convexHull(rightEye)], -1, (0, 255, 0), 1)
        
        # Draw face rectangle
        cv2.rectangle(frame, (rect.left(), rect.top()), 
                     (rect.right(), rect.bottom()), (255, 0, 0), 2)
        
        break  # Use only the first valid face detected
    
    # --- Blink Detection Logic ---
    if valid_face_detected:
        current_time = time.time()
        
        if current_ear < EYE_AR_THRESH:
            FRAME_COUNTER += 1
        else:
            # If the eyes were closed for a sufficient number of frames
            # and not too long (to avoid counting long closures)
            if (EYE_AR_CONSEC_FRAMES <= FRAME_COUNTER <= MAX_BLINK_FRAMES and 
                current_time - last_blink_time > BLINK_COOLDOWN):
                message = "true".encode('utf-8')
                sock.sendto(message, (UDP_IP, UDP_PORT))
                TOTAL_BLINKS += 1
                last_blink_time = current_time
                print(f"Blink Detected! Total: {TOTAL_BLINKS}")
                
                # Visual feedback for blink detection
                cv2.circle(frame, (50, 50), 20, (0, 255, 0), -1)
            
            # Reset the frame counter
            FRAME_COUNTER = 0
        
        # Status indicator
        status = "EYES CLOSED" if current_ear < EYE_AR_THRESH else "EYES OPEN"
        color = (0, 0, 255) if current_ear < EYE_AR_THRESH else (0, 255, 0)
        cv2.putText(frame, status, (10, 60), cv2.FONT_HERSHEY_SIMPLEX, 0.6, color, 2)
    else:
        # No valid face detected, reset counters
        FRAME_COUNTER = 0
        ear_history.clear()
        cv2.putText(frame, "NO VALID FACE DETECTED", (10, 60),
                   cv2.FONT_HERSHEY_SIMPLEX, 0.6, (0, 0, 255), 2)
    
    # Display information on frame
    cv2.putText(frame, f"Blinks: {TOTAL_BLINKS}", (10, 30),
                cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
    
    if valid_face_detected:
        cv2.putText(frame, f"EAR: {current_ear:.3f}", (300, 30),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, (255, 255, 255), 2)
        cv2.putText(frame, f"Threshold: {EYE_AR_THRESH}", (300, 60),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
        cv2.putText(frame, f"Frames: {FRAME_COUNTER}", (300, 80),
                    cv2.FONT_HERSHEY_SIMPLEX, 0.5, (255, 255, 255), 1)
    
    # Show the frame
    cv2.imshow("Blink Detection", frame)
    key = cv2.waitKey(1) & 0xFF
    
    # Handle key presses
    if key == ord("q"):
        break
    elif key == ord("r"):
        TOTAL_BLINKS = 0
        FRAME_COUNTER = 0
        ear_history.clear()
        print("Blink counter reset")

# --- Cleanup ---
print(f"Final blink count: {TOTAL_BLINKS}")
cap.release()
cv2.destroyAllWindows()
sock.close()
