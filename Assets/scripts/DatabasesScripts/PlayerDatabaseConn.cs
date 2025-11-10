using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System;

namespace DatabasesScripts
{
    public class PlayerDatabaseConn
    {
        private string dbPath;
        private SqliteConnection conn;
        private int playerCharacterId;
        
        public PlayerDatabaseConn()
        {
            // Use helper to get correct database path for both Editor and Build
            dbPath = DatabasePathHelper.GetDatabasePath();
            conn = new SqliteConnection(dbPath);
            
            Debug.Log($"[PlayerDatabaseConn] Connecting to database at: {dbPath}");
            
            try
            {
                conn.Open();

                SqliteCommand cmd = conn.CreateCommand();
                
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT characterId FROM Characters " + 
                                  "WHERE characterName = @characterName";
                cmd.Parameters.Add(new SqliteParameter
                    {
                        ParameterName = "characterName",
                        Value = "PlayerCharacter"
                    }
                );

                SqliteDataReader result = cmd.ExecuteReader();
                result.Read();
                playerCharacterId = result.GetInt32(0);
                conn.Close();
                
                Debug.Log($"[PlayerDatabaseConn] Successfully loaded player character ID: {playerCharacterId}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PlayerDatabaseConn] CRITICAL ERROR - Failed to connect to database: {e.Message}");
                Debug.LogError($"[PlayerDatabaseConn] Stack trace: {e.StackTrace}");
                throw;
            }
        }

        public void SetMoveSpeed(float newMoveSpeed)
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();

            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "UPDATE Characters " + 
                              "SET characterMoveSpeed = @newMoveSpeed " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
            {
                ParameterName = "newMoveSpeed",
                Value = newMoveSpeed
            });
            cmd.Parameters.Add(new SqliteParameter
            {
                ParameterName = "characterId",
                Value = playerCharacterId
            });

            cmd.ExecuteNonQuery();
            
            conn.Close();
        }

        public int GETPlayerCharacterId()
        {
            return playerCharacterId;
        }

        public float GETPlayerMoveSpeed()
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();
            
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT characterMoveSpeed FROM Characters " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "characterId",
                    Value = playerCharacterId
                }
            );

            var result = cmd.ExecuteReader();
            result.Read();
            float moveSpeed = result.GetFloat(0);
            
            conn.Close();

            return moveSpeed;
        }

        public string GETDbPath()
        {
            return dbPath;
        }

        public float GETPlayerHealth()
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();
            
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT characterHealth FROM Characters " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "characterId",
                    Value = playerCharacterId
                }
            );

            var result = cmd.ExecuteReader();
            result.Read();
            float health = result.GetFloat(0);
            
            conn.Close();

            return health;
        }

        public float GETPlayerAttackDamage()
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();
            
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT characterAttackDamage FROM Characters " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "characterId",
                    Value = playerCharacterId
                }
            );

            var result = cmd.ExecuteReader();
            result.Read();
            float attackDamage = result.GetFloat(0);
            
            conn.Close();

            return attackDamage;
        }
        
        public float GETPlayerAttackRange()
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();
            
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT characterAttackRange FROM Characters " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "characterId",
                    Value = playerCharacterId
                }
            );

            var result = cmd.ExecuteReader();
            result.Read();
            float attackRange = result.GetFloat(0);
            
            conn.Close();

            return attackRange;
        }
        
        public float GETPlayerAttackCooldown()
        {
            conn.Open();

            SqliteCommand cmd = conn.CreateCommand();
            
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "SELECT characterAttackCooldown FROM Characters " + 
                              "WHERE characterId = @characterId";
            cmd.Parameters.Add(new SqliteParameter
                {
                    ParameterName = "characterId",
                    Value = playerCharacterId
                }
            );

            var result = cmd.ExecuteReader();
            result.Read();
            float attackCooldown = result.GetFloat(0);
            
            conn.Close();

            return attackCooldown;
        }
    }
}