// MongoDB initialization script
db = db.getSiblingDB('PersonalizedAssistant');

// Create collections with proper indexes
db.createCollection('users');
db.createCollection('data_collections');
db.createCollection('chat_sessions');
db.createCollection('gmail_data');
db.createCollection('google_drive_data');
db.createCollection('ios_contact_data');
db.createCollection('ios_calendar_data');

// Create indexes for better performance
db.users.createIndex({ "googleId": 1 }, { unique: true });
db.users.createIndex({ "email": 1 });
db.users.createIndex({ "createdAt": 1 });

db.data_collections.createIndex({ "userId": 1 });
db.data_collections.createIndex({ "source": 1 });
db.data_collections.createIndex({ "status": 1 });
db.data_collections.createIndex({ "collectedAt": 1 });

db.chat_sessions.createIndex({ "userId": 1 });
db.chat_sessions.createIndex({ "sessionId": 1 }, { unique: true });
db.chat_sessions.createIndex({ "lastActivityAt": 1 });

db.gmail_data.createIndex({ "userId": 1 });
db.gmail_data.createIndex({ "threadId": 1 });
db.gmail_data.createIndex({ "sentAt": 1 });

db.google_drive_data.createIndex({ "userId": 1 });
db.google_drive_data.createIndex({ "lastModified": 1 });

db.ios_contact_data.createIndex({ "userId": 1 });
db.ios_contact_data.createIndex({ "firstName": 1, "lastName": 1 });

db.ios_calendar_data.createIndex({ "userId": 1 });
db.ios_calendar_data.createIndex({ "startTime": 1 });
db.ios_calendar_data.createIndex({ "endTime": 1 });

print('Database initialized successfully');
