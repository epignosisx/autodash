for /l %%x in (1, 1, 10) do ( 
	echo %%x
	..\packages\xunit.runner.console.2.0.0\tools\xunit.console.exe ..\tests\Autodash.Core.Tests\bin\Debug\Autodash.Core.Tests.dll -class "Autodash.Core.Tests.ParallelSuiteRunRunnerTests"
)