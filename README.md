# Ketchup - IDE & Programming Language
Project Start June-July 2024
Project End Approx. April-May 2025

Project Name: Ketchup Programming Language + IDE
Using Visual Studio - Windows Forms (C#)

Objectives:
Core:
1. Provide the user with a clear error detection system.
The program should identify the error and display a clear explanation for why the program isn’t running. The explanation should be in simple terms and should point the user towards the line (or a specific lexeme), where the error is. To measure the success of this error detection system, it should be tested with a variety of errors, and it must identify those errors correctly. 
The effectiveness of the error detection system should be tested by observing user’s ability to understand the error when they occur. This can be done by providing programs with predetermined errors and see how effectively the students can understand and therefore fix the error. Specifically, testing this by taking 2 groups of students and giving one of them access to the error detection system and giving the other group (control) an IDE without an error detection system enabled. Comparing the time in which each group of students fix the error will allow me to gauge the success of the error detection system.
2. Users should be able to save, open and execute their programs.
The programming language should have its own custom file format. The save and open actions should be easily accessible in 2 clicks or via key bind. To verify that these functions are easy to execute, I would observe how a user saves and opens files.
Executing user’s programs should be easy to do and should be achievable in 1 click. If the program is not able to be fully executed, then the user should get a prompt and should be able to recover their program without restarting the entire application. The program should be run in a console window and should not be crashed by the user. This can be tested by observing how well users are able to recover their program after the programs fail to run due to an error.
3. The UI should not be complicated.
The UI would be deemed easy to understand if all the features are accessible in less than 3 clicks. This will be achieved by having all the features in the tool bar. Easy execution of common actions should be achievable through keyboard shortcuts. The UI should be similar to popular IDEs since they have a tried and tested layout, this will make switching from platforms easier and smoother, improving the testing process. Testing the ease of use of the UI will be done through observation of the navigation of the users through the IDE.
4. Syntax highlighting
The different lexemes of the programming language should be highlighted in different colours. The syntax should be similar to Python, Lua or JavaScript’s, since these are tried and tested syntax patterns. To verify the usefulness of the syntax highlighting, I could conduct a test about the user’s understanding of the different token types. I would do this by comparing how successfully students are able to pick out different tokens with and without syntax highlighting. The success of the syntax highlighting can be judged by comparing the time it takes for the average student to point out the different lexemes.
5. The IDE should provide easy to understand error messages.
The error messages provided by the IDE should be easy to understand for a user in the age range of 11-14 years old. When a program fails to fully execute, the error message should be displayed in a easily accessible area of the IDE. The ease of understanding an error message can be judged by observing how a user reacts to an error message, whether the user is able to understand and act on the information provided.
6. The IDE should allow for easy debugging.
The IDE should be able to assist the users with debugging by providing debugging tools. The debugging tools should be accented on (brought to attention) when a program written by the user fails to run. Testing the success of these tools can be done by demonstrating how to use the tools to the students and then seeing if they are able to find usefulness from any of the tools. Alternatively, to measure the success of the tools, I could simply observe the user’s interactions with the tools when attempting to fix their code.
7. The syntax should be readable and understandable to an English-speaker.
Any given program should be readable and easily understandable to an average English-speaker in the range of 16-18 years old. This can be tested by writing a relatively complex program and having a volunteer, who has not study CS (Computer Science) since Year 9, explain what the program is trying to do. However, it must be noted that the algorithm or program which is used for this test should be easily understandable from a conceptual level. A simple example of a program of this sort would be a program that displays numbers 1-10. However, a program that would not be a good fit for this test would be a sorting algorithm, since a non-CS student would not be able to easily conceptualise that, even if it was written in plain English. 
A correctly written program, that is not conceptually challenging, should be easily understood by an average A-Level student not taking Computer Science.
In order to test this, I will ask such a student to explain what each line of code does, in the process testing if the programming language easily translates into English. A simple program should be used such as a program which decides if a number is even or odd.
8. The language should be Turning complete.
The programming language should be Turing complete. In order to be Turing complete the language must have a working form of conditional repetition (while) or conditional jump (if + goto) and must have a way to read and write some form of storage (e.g. variables or tape).
Extensions:
1. Provide the user with a reasonable correction for their syntactical error.
This IDE should be able to give a reasonable response and in some cases an explanation for the mistake/error and provide a suggested correction for it, similar to the way that Visual Studio attempts to provide multiple fixes to the user’s code. To measure the success of this system, it should be able to correct basic mistakes made by users and it can be tested with different samples of code with various mistakes. The correction system will be deemed successful if it can correct around ⅔ of the programs provided.
2. Provide a system which will help complete words (IntelliSense).
The IDE should have an intelligent suggestion system which helps the user write programs faster and more efficiently. This system should make learning the language quicker. This could be measured by tasking students of equal skill to write a program of equal difficulty and giving half the students this assisting system and giving the other half no assistance, comparing the time difference will give a good overview of the success of this system. 
3. Implementation of a visual drawing tool.
The programming language should be able to support a drawing tool, similar to Python’s turtle library. The tool should be able to create basic geometric shapes by taking in simple commands such as: FORWARD X, TURN θ (where θ is in degrees), LIFT PEN.
To measure the success of this tool, a user should be able to draw regular shapes and irregular shapes of reasonable dimensions.
4. Implement recursive functions.
The programming language should be able to support recursive algorithms, such as the Merge sort or the recursive alternative of an algorithm for finding the Fibonacci sequence. 
To measure the success of this a program such as the merge sort should be able to run at a similar efficiency to python in terms of CPU ticks.
