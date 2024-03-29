﻿Feature: Defining the characteristics of the zone when a player moves around the map

Scenario: #1 Player attempts to move one room north on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves North 1 room
	Then the player should be N1 of its origin

Scenario: #2 Player attempts to move two rooms north on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves North 2 rooms
	Then the player should be N1 of its origin

Scenario: #3 Player attempts to move two rooms south on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves South 2 rooms
	Then the player should be S1 of its origin

Scenario: #4 Player attempts to move two rooms east on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves East 2 rooms
	Then the player should be E1 of its origin

Scenario: #5 Player attempts to move two rooms west on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves West 2 rooms
	Then the player should be W1 of its origin

Scenario: #6 Player attempts to move two rooms west and one room north on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves West 2 rooms
	And the player moves North 1 room
	Then the player should be W1|N1 of its origin

Scenario: #7 Player attempts to move one room west and one room north and two rooms east and two rooms south on a cube zone
	Given I have a player
	And I have a cube zone
	And my player is at the center of the cube zone
	When the player moves West 2 rooms
	And the player moves North 1 room
	And the player moves East 2 room
	And the player moves South 2 room
	Then the player should be E2|S2 of its origin

Scenario: #8 Player attempts to move west while in a north to south hallway
	Given I have a player
	And the player is in a north to south hallway
	And my player is at the center of the North to South Hallway zone
	When the player moves West 2 rooms
	Then the player should be at its origin

Scenario: #9 Player attempts to move east while in a north to south hallway
	Given I have a player
	And the player is in a north to south hallway
	And my player is at the center of the North to South Hallway zone
	When the player moves East 2 rooms
	Then the player should be at its origin