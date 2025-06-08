// src/firebase/config.js
import { initializeApp } from 'firebase/app';
import { getFirestore } from 'firebase/firestore';
import { getStorage } from 'firebase/storage';
import { getAnalytics } from 'firebase/analytics';

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

// Khởi tạo dịch vụ
const db = getFirestore(app);
const storage = getStorage(app);
const analytics = getAnalytics(app);

export { db, storage, analytics };