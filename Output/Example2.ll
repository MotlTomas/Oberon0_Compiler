; ModuleID = 'ArrayDemo'
source_filename = "ArrayDemo"
target triple = "x86_64-pc-linux-gnu"

@mat = global i8* null
@res = global i8* null

declare i32 @printf(i8*, ...)

declare i32 @scanf(i8*, ...)

declare i32 @puts(i8*)

define void @FillMatrix(i8**) {
entry:
  %i = alloca i64
  %j = alloca i64
  ret void
}

define i64 @SumMatrix(i8*) {
entry:
  %i = alloca i64
  %j = alloca i64
  %sum = alloca i64
  store i64 0, i64* %sum
  %sum1 = load i64, i64* %sum
  ret i64 %sum1
}

define i32 @main() {
entry:
  %mat = load i8*, i8** @mat
  call void @FillMatrix(i8* %mat)
  %mat1 = load i8*, i8** @mat
  store i8* %mat1, i8** @res
  ret i32 0
}
