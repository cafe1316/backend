import admin from "firebase-admin";

if (!admin.apps.length) {
  const sa = process.env.FIREBASE_SERVICE_ACCOUNT_JSON
    ? JSON.parse(process.env.FIREBASE_SERVICE_ACCOUNT_JSON)
    : undefined;

  admin.initializeApp({
    credential: sa ? admin.credential.cert(sa) : admin.credential.applicationDefault(),
  });
}

export const firebaseAdmin = admin;