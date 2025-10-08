#!/usr/bin/env python3
"""
MongoDB Connection Test Script
Run this to verify your connection string works
"""

from pymongo import MongoClient
import sys

# Your connection string from appsettings.Development.json
connection_string = "mongodb+srv://thevindualgo_db_user:uC8dyX1CQHLMaXW3@cluster0.cub8yqz.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0"

try:
    print("Testing MongoDB connection...")
    client = MongoClient(connection_string, serverSelectionTimeoutMS=5000)
    
    # Test connection
    client.admin.command('ping')
    print("✅ Connection successful!")
    
    # Test database access
    db = client.evcs_dev
    collections = db.list_collection_names()
    print(f"✅ Database access successful! Collections: {collections}")
    
except Exception as e:
    print(f"❌ Connection failed: {e}")
    print("\nPossible solutions:")
    print("1. Check your username/password in MongoDB Atlas")
    print("2. Whitelist your IP address in MongoDB Atlas Network Access")
    print("3. Verify database permissions for the user")

client.close() if 'client' in locals() else None