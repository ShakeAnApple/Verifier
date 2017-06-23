# Верификатор LTL формул

Проект по дисциплине "Верификация моделей программ" 

Выполнили: Овсянникова Полина(группа M4136c), Виктор Петухов(группа M4138)

Исходные данные: автомат Харела и LTL формула

# Описание проекта

Верификация программы состоит из следующих этапов:
1) Представление исходного автомата во внутреннюю модель верификатора
2) Преобразование модели программы в автомат Бюхи
3) Построение автомата Бюхи по негативной нормальной форме заданного LTL свойства
4) Пересечение полученных автоматов, поиск циклов. Если цикл найден, значит пересечение языка исходного автомата и языка автомата, построенного по отрицанию LTL свойства, непустое, следовательно, заданное LTL свойство не выполняется.

### Примечания:
- построение автомата Бюхи по LTL формуле производится с помощью стороннего инструмента http://www.lsv.fr/~gastin/ltl2ba/index.php.
В проекте содержатся его скомпилированные под 32-разрядные Linux и Windows исполняемые файлы.


# Сборка

Выполнить в корне 
```
make
```

# Использование

### Верификатор поддерживает следующий формат LTL формул:
Унарные операторы:
* `[]`	(Globally (always))
* `<>`	(Future (eventually))
* `!` 	(отрицание)

Бинарные операторы:
* `U` 	(Until)
* `V` 	(Release)
* `&&`	(логическое И)
* `||`	(логическое ИЛИ)
* `->`	(Следование)
* `<->`	(Эквивалентность)
  
### Запуск
```
$ verifier -h
Usage :
                Verifier.exe [options]

where

options are
        -m <arg>
        --model-automaton-file <arg>
                File with automaton for verification, otherwise interactive mode
        -f <arg>
        --ltl-formula <arg>
                Ltl formula to verificate on model (if loaded), or interactive mode
        -b <arg>
        --batch-test <arg>
                Run commands or Ltl formulas sequence from specified file
        -md <arg>
        --model-diagram <arg>
                Save model (if loaded) automaton diagram to dgml file
        -fd <arg>
        --formula-diagram <arg>
                Save Ltl formula (if presented) automaton diagram to dgml file
        -vd <arg>
        --verifier-diagram <arg>
                Save virifier automaton (m and f intersection) diagram to dgml file
        -h
        --help
                Show help (or just 'help' in interactive mode)
```
Интерактивный режим:
```
$ verifier
Input LTL-formula or command (help for commands list or exit):
> help

Available commands:
    help                         - show this help
    load <model.xstd>            - load model automaton for verification
    run  <ltlexprs.txt>          - run commands or Ltl formulas sequence from specified file
    save model <model.dgml>      - save last model automaton diagram to dgml file
    save formula <formula.dgml>  - save last Ltl formula automaton diagram to dgml file
    save verifier <verfier.dgml> - save last virifier automaton diagram to dgml file
    exit                         - exit from interactive mode

>
```
