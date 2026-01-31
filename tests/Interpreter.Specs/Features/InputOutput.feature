#language: ru
Функциональность: ввод-вывод и завершение программы

    Сценарий: вывод чисел функцией printi
        Пусть я загрузил программу "features/input_output/print_int.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        1337
        """

    Сценарий: вывод текста функцией print
        Пусть я загрузил программу "features/input_output/print_string.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        Tyger Tyger, burning bright,
        In the forests of the night;
        What immortal hand or eye,
        Could frame thy fearful symmetry?
        """

    Сценарий: завершение из программы функцией exit
        Пусть я загрузил программу "features/input_output/exit.tig"

        Когда я выполняю программу

        Тогда я увижу вывод:
        """
        13
        """

    Сценарий: посимвольное чтение ввода
        Пусть я загрузил программу "features/input_output/getchar_5_times.tig"

        Когда я ввожу текст:
            """
            Tyger, Tyger
            """
        И выполняю программу

        Тогда я увижу вывод:
        """
        Tyger
        """
        