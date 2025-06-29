// firebase/rtdb-config.js
import { initializeApp } from 'firebase/app';
import { 
  getDatabase,
  ref,
  onValue,
  set,
  push,
  serverTimestamp,
  off,
  get,
  update
} from 'firebase/database';
import { getStorage } from 'firebase/storage';

// Cấu hình Firebase - thay bằng config của bạn
const firebaseConfig = {
  apiKey: "AIzaSyDPxXUrCP-Juhj1kTGIflfbrb66_97MrCI",
  authDomain: "web-app-c1fa1.firebaseapp.com",
  databaseURL: "https://web-app-c1fa1-default-rtdb.asia-southeast1.firebasedatabase.app",
  projectId: "web-app-c1fa1",
  storageBucket: "web-app-c1fa1.firebasestorage.app",
  messagingSenderId: "606306901710",
  appId: "1:606306901710:web:ebecaec41d0b89be5dfa9f",
  measurementId: "G-Y8K58MXYP0"
};

// Khởi tạo Firebase
const app = initializeApp(firebaseConfig);

// Khởi tạo Realtime Database
export const rtdb = getDatabase(app);

// Khởi tạo Storage
export const storage = getStorage(app);

// Export các functions từ Firebase Realtime Database
export { 
  ref as dbRef,
  onValue,
  set,
  push,
  serverTimestamp as rtdbServerTimestamp,
  off,
  get,
  update
};