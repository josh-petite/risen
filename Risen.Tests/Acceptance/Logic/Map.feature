Feature: Defining the characteristics of the map when a player moves around	

Scenario: #1 Player attempts to move one room north on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves north 1 room
	Then the player should be N1 of its origin

Scenario: #2 Player attempts to move two rooms north on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves north 2 rooms
	Then the player should be N1 of its origin

Scenario: #3 Player attempts to move two rooms south on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves south 2 rooms
	Then the player should be S1 of its origin

Scenario: #4 Player attempts to move two rooms east on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves east 2 rooms
	Then the player should be E1 of its origin

Scenario: #5 Player attempts to move two rooms west on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves west 2 rooms
	Then the player should be W1 of its origin

Scenario: #6 Player attempts to move two rooms west and one room north on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves west 2 rooms
	And the player moves north 1 room
	Then the player should be W1|N1 of its origin

Scenario: #7 Player attempts to move one room west and one room north and two rooms east and two rooms south on a cube map
	Given I have a character
	And I have a cube map
	And my player is at the center of the cube map
	When the player moves west 2 rooms
	And the player moves north 1 room
	And the player moves east 2 room
	And the player moves south 2 room
	Then the player should be E2|S2 of its origin

Scenario: #8 Player attempts to move west while in a north to south hallway
	Given I have a character
	And the player is in a north to south hallway
	And my player is at the center of the North to South Hallway map
	When the player moves west 2 rooms
	Then the player should be at its origin

Scenario: #9 Player attempts to move east while in a north to south hallway
	Given I have a character
	And the player is in a north to south hallway
	And my player is at the center of the North to South Hallway map
	When the player moves east 2 rooms
	Then the player should be at its origin