# Ketchup - IDE & Programming Language
Project Start June-July 2024
Project End Approx. 29th April 2025

Project is written in Visual Studio using Windows Forms (C#)

**Objectives:**
**1. Provide the user with a clear error detection system.**
The program should identify the error and display a clear explanation for why the program isn’t running. The explanation should be in simple terms (simple enough for a 11–14-year-old to understand) and should guide the user towards the line (or a specific lexeme), where the error is. To measure the success of this error detection system, it should be tested with a variety of errors, and it must identify those errors correctly. 
The effectiveness of the error detection system should be tested by observing user’s ability to understand the error when they occur. This can be done by providing programs with predetermined errors and see how effectively the students can understand and therefore fix the error. Alternatively, this could be tested by seeing how the users deal with fixing their own errors, when generally programming.
**2. Users should be able to save, open and execute their programs.**
The programming language should have its own custom file format. The save and open actions should be easily accessible in 2 clicks or via key bind. To verify that these functions are easy to execute, I would observe how a user saves and opens files.
Executing user’s programs should be easy to do and should be achievable in 1 click. If the program is not able to be fully executed, then the user should get a prompt and should be able to recover their program without restarting the entire application. The program should be run in a console and should not be crashed by the user. This can be tested by observing how well users are able to recover their program after the programs fail to run due to an error.
**3. The UI should not be complicated.**
The UI would be deemed easy to understand if all the features are accessible in less than 3 clicks. This will be achieved by having all the features in the tool bar. Easy execution of common actions should be achievable through keyboard shortcuts or easily navigable buttons. The UI should be similar to popular IDEs since they have a tried and tested layout, this will make switching from platforms easier and smoother, improving the testing process. Testing the ease of use of the UI will be done through observation of the navigation of the users through the IDE.
**4. Syntax highlighting**
The different lexemes of the programming language should be highlighted in different colours. The syntax should be similar to Python, Lua or JavaScript’s, since these are tried and tested syntax patterns. To verify the usefulness of the syntax highlighting, I could conduct a test about the user’s understanding of the different token types. I would do this by comparing how successfully students are able to pick out different tokens with and without syntax highlighting. The success of the syntax highlighting can be judged by comparing the time it takes for the average student to point out the different lexemes.
**5. The syntax should be readable and understandable to an English-speaker.**
Any given program should be readable and easily understandable to an average English-speaker in the range of 16-18 years old. This can be tested by writing a relatively complex program and having a volunteer, who has not study CS (Computer Science) since Year 9, explain what the program is trying to do. However, it must be noted that the algorithm or program which is used for this test should be easily understandable from a conceptual level. A simple example of a program of this sort would be a program that displays numbers 1-10. However, a program that would not be a good fit for this test would be a sorting algorithm, since a non-CS student would not be able to easily conceptualise that, even if it was written in plain English. 
A correctly written program, that is not conceptually challenging, should be easily understood by an average A-Level student not taking Computer Science.
To test this, I will ask such a student to explain what each line of code does, in the process testing if the programming language easily translates into English. A simple program should be used such as a program which decides if a number is even or odd.
**6. The language should be Turning complete.**
The programming language should be Turing complete. In order to be Turing complete the language must have a working form of conditional repetition (while) or conditional jump (if + goto) and must have a way to read and write some form of storage (e.g. variables or tape).
**7. The language should have a low barrier to entry.**
The programs writing in this language should not contain excess boilerplate code, such as the main Program class structure or a Main subroutine as seen in many C-family languages. The language should also support as many simple to understand structures as possible (different loops, if statements, etc.).
To test this a newly introduced user should be able to write a simple program such as a “Hello, World” program or a loop counting to 10. This can also be compared to the time taken to write the equivalent program in another beginner-friendly language (e.g. Python).
**Extensions:**
**1. Provide the user with a reasonable correction for their syntactical error.**
This IDE should be able to give a reasonable response and in some cases an explanation for the mistake/error and provide a suggested correction for it, similar to the way that Visual Studio attempts to provide multiple fixes to the user’s code. To measure the success of this system, it should be able to correct basic mistakes made by users and it can be tested with different samples of code with various mistakes. The correction system will be deemed successful if it can correct around ⅔ of the programs provided.
**2. Provide a system which will help complete words (IntelliSense).**
The IDE should have an intelligent suggestion system which helps the user write programs faster and more efficiently. This system should make learning the language quicker. This could be measured by tasking students of equal skill to write a program of equal difficulty and giving half the students this assisting system and giving the other half no assistance, comparing the time difference will give a good overview of the success of this system. 
**3. Implementation of a visual drawing tool.**
The programming language should be able to support a drawing tool, similar to Python’s turtle library. The tool should be able to create basic geometric shapes by taking in simple commands such as: FORWARD X, TURN θ (where θ is in degrees), LIFT PEN.
To measure the success of this tool, a user should be able to draw regular shapes and irregular shapes of reasonable dimensions.
**4. Implement recursive functions.**
The programming language should be able to support recursive algorithms, such as the Merge sort or the recursive alternative of an algorithm for finding the Fibonacci sequence. 
To measure the success of this a program such as the merge sort should be able to run at a similar efficiency to python in terms of CPU ticks.

**Evaluation**
**1. Provide the user with a clear error detection system.**
The error detection system catches the vast majority of the errors, handling the errors by displaying an appropriate error message in a console, which has been tailored to provide only the necessary information.
 
Mentioning the type of error (Syntax), where the error is (Line 1), followed by as much information that the interpreter can provide from the information it has. The error detection system is unfortunately a lot weaker in the specifics when it comes to logic errors, since it deals with the execution in intermediate code, and it cannot quickly trace back the error to the original code.
When asking students, they commented on the system and mostly liked the simplicity of the error message, being able to understand where the error message is and what exactly they need to fix on that line. However, they did mention a possible improvement, which involved highlighting in red the exact part of the code where the error occurred.
This objective was completed to a satisfactory standard as it does its job effectively enough to lead me to encounter 2 actual application crashes (which were not handled by the error detection) in the time of 3 testing phases.
**2. Users should be able to save, open and execute their programs.**
The program currently supports the .ktch file extension. The files are openable in the Ketchup IDE. The saving, opening and execution can be done through buttons on the menu strip and shortcuts. The data being saved is raw .txt files, but when the program is loaded into the IDE, the syntax highlighting is done in the text console. When testing with some users I noticed that saving files was easy for the students to understand and do. All failed execution can be recovered, since when a program fails to execute an error message is displayed in the console and the program’s execution is stopped.
This objective was achieved very well, meeting the expectations set in the objective definition.
**3. The UI should not be complicated.**
All features in the program are accessible within 3 or less clicks. Another good indication for a good UI is that it ended up looking similar to Thonny the python IDE. The general UI cleanliness and uncomplicated nature was also achieved through shortcuts, which give quick access to less frequently used functions, such as the Token and Intermediate View, which are useful for language development rather than actual general use of the Ketchup language. Therefore, this objective is achieved very well, additionally supported by the fact that no users ever questioned how any of the UI worked, using all the UI intuitively.
**4. Syntax highlighting**
The syntax highlighting was implemented smoothly, it does not affect performance of the program, and it completes it function. When asking students who were newly introduced to the programming language, they said that the syntax highlighting helped break up the text for them (“categorising the words”). 
Additionally, the syntax highlighting helps by communicating to the user when a lexeme is correctly typed, since the word will become highlighted in the appropriate colour, this then translates to helping them in situation where they misspell a lexeme, where they notice that the word they had just written is not highlighted in a new colour, making them notice the mistake in a more obvious way. 
**5. The syntax should be readable and understandable to an English-speaker.**
When testing a simple program with a non-computer science student, they could easily identify the function and output of programs. Below is an example of two interpretations of simple programs.
COUNT WITH i FROM 1 TO 10
  PRINT i
END COUNT
_“This program iterates from 1 to 10, stepping by 1 each time, printing each individual output. Therefore, the output is:
1
2
3
4
5
6
7
8
9
10”_ – Thomas, Year 13
REPEAT 5 TIMES
  PRINT "Hello"
END REPEAT
_“This program prints ‘Hello’ 5 times. The output of this program would be:
Hello
Hello
Hello
Hello
Hello
Hello”_ – Thomas, Year 13
This demonstrates that the syntax can be easily understood by someone who known English to a proficient level. Meaning the objective has been achieved well, however more abstracting of certain structures could make it better, especially for concepts which even some computer scientists struggle with (e.g. functions, which were not a big focus since it is not as necessary of a structure for KS3 as loops and selection statements).
**6. The language should be Turning complete.**
The language is Turing complete. This can be simply seen by seeing the execution of a relatively complex program (such as a bubble sort), which uses all the structures assumed by a Turing complete language. Therefore, the objective has been met successfully. However, this is the most clearly defined objective. Therefore, no user testing is required.
**7. The language should have a low barrier to entry.**
The language has a low barrier to entry, because even Year 7 students, who had never programmed in text-based languages (e.g. Python, Pascal), could program in the language after a few minutes of explaining the syntax. This means that the language is easy to adapt to and is also similar enough to English. This was specifically reflected when I did an exercise with the students where I would explain a program to them in English and they could effectively translate the concept into code, because it was so close to English.
Example of Ketchup’s parity with English:
**Count**ing **from 1 to 10** and **print**ing every number
COUNT WITH i FROM 1 TO 10
  PRINT i
END COUNT
**8. Provide the user with a reasonable correction for their syntactical error. (Extension)**
This was implemented unintentionally, making the error detection system write the error messages in a way that suggest a correction.
 
 
Due to the way error detection is constructed, it leads to giving the user the token which is needed in that situation. Avoiding using complicated language or overly generalised prompts such as the one used in Python’s error messages.
**9. Provide a system which will help complete words (IntelliSense). (Extension)**
The IntelliSense greatly improves typing efficiency in Ketchup. This is backed up by the data recorded and analysed during the testing section of this document. The testing is an effective way of testing, since it is backed up by statistics. The IntelliSense is an autocompletion box, which pops up when a word is being typed, giving suggestions as to what keywords can be insert in replacement for it. This allows long keywords to be quickly typed, as in the testing phase the keyword FUNCTION was easily typed with 3 key clicks (“F”, “u”, TAB). 
When testing the program with the users, they said the autocomplete feature is useful and helped speed up their typing. In fact, some of the students picked up on the feature without me even mentioning it.
From this it is fair to say the objective was completed effectively. 
10. Implementation of a visual drawing tool. (Extension)
A visual drawing tool was not developed or implemented in the end, this was because a lot more time was spent optimizing the syntax and adding certain parts of the language then initially expected.
Additionally, the addition of a drawing tool would not have been as useful as the implementation of lists, which took up the time initially given to implementing the drawing tool. Instead of rushing to make the drawing tool, more time was invested into make the list implementation as consistent as possible, since it is integral to running algorithms, such as sorting algorithms.
Therefore, the objective was not met, but the time allocated for it was spent productively making another part of the language better.
1**1. Implement recursive functions. (Extension)**
Whilst this feature was not implemented, the framework for its implementation effectively exists. The design of the language was built around the idea of recursion, since the subroutines are run as stack frames, rather than just running another piece of code separately, they are all linked using the call stack, keeping a consistent flow of subroutine calls. However, for some undiscovered reasons a fully working implementation of recursion was not made. Although if more time was allocated to this, then this could probably be slightly rewritten and made functional. In the end, this objective was not met, but due to good planning in the design this feature could potentially be added with a little more work.
