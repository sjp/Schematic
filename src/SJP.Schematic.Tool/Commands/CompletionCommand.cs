using System;
using System.ComponentModel;
using System.Threading;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

[Description("Generate shell completion scripts for the schematic CLI.")]
internal sealed class CompletionCommand : Command<CompletionCommand.Settings>
{
    public enum ShellType
    {
        Bash,
        Zsh,
        Fish,
        PowerShell
    }

    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<SHELL>")]
        [Description("The shell to generate completions for. One of: bash, zsh, fish, powershell.")]
        public ShellType Shell { get; init; }
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var script = GetCompletionScript(settings.Shell);

        // Write directly to stdout rather than via IAnsiConsole so the emitted
        // script is byte-for-byte exact (no markup parsing or line wrapping),
        // which matters when the output is redirected to a file or sourced.
        Console.Out.WriteLine(script);

        return ErrorCode.Success;
    }

    internal static string GetCompletionScript(ShellType shell)
    {
        return shell switch
        {
            ShellType.Bash => BashScript,
            ShellType.Zsh => ZshScript,
            ShellType.Fish => FishScript,
            ShellType.PowerShell => PowerShellScript,
            _ => throw new ArgumentOutOfRangeException(nameof(shell), shell, "Unknown shell type.")
        };
    }

    private const string BashScript =
        """
        # bash completion for schematic
        #
        # Enable for the current shell:
        #   source <(schematic completion bash)
        # Enable permanently:
        #   schematic completion bash > /etc/bash_completion.d/schematic
        _schematic() {
            local cur cmd subcmd i w
            cur="${COMP_WORDS[COMP_CWORD]}"

            local commands="orm lint report test completion"
            local global="-h --help"

            cmd=""
            subcmd=""
            for (( i=1; i < COMP_CWORD; i++ )); do
                w="${COMP_WORDS[i]}"
                [[ "$w" == -* ]] && continue
                if [[ -z "$cmd" ]]; then
                    cmd="$w"
                elif [[ -z "$subcmd" ]]; then
                    subcmd="$w"
                fi
            done

            case "$cmd" in
                "")
                    COMPREPLY=( $(compgen -W "$commands -v --version $global" -- "$cur") )
                    ;;
                orm)
                    if [[ -z "$subcmd" ]]; then
                        COMPREPLY=( $(compgen -W "efcore ormlite poco $global" -- "$cur") )
                    else
                        COMPREPLY=( $(compgen -W "-c --config --convention --project-path --base-namespace $global" -- "$cur") )
                    fi
                    ;;
                lint)
                    COMPREPLY=( $(compgen -W "-c --config $global" -- "$cur") )
                    ;;
                report)
                    COMPREPLY=( $(compgen -W "-c --config --output $global" -- "$cur") )
                    ;;
                test)
                    COMPREPLY=( $(compgen -W "-c --config -t --timeout $global" -- "$cur") )
                    ;;
                completion)
                    COMPREPLY=( $(compgen -W "bash zsh fish powershell $global" -- "$cur") )
                    ;;
                *)
                    COMPREPLY=( $(compgen -W "$global" -- "$cur") )
                    ;;
            esac
        }
        complete -F _schematic schematic
        """;

    private const string ZshScript =
        """
        #compdef schematic
        #
        # zsh completion for schematic
        #
        # Enable for the current shell:
        #   source <(schematic completion zsh)
        # Enable permanently, save to a directory on your $fpath, e.g.:
        #   schematic completion zsh > "${fpath[1]}/_schematic"

        autoload -U is-at-least

        _schematic() {
            local curcontext="$curcontext" state line
            typeset -A opt_args

            _arguments -C \
                '1: :->command' \
                '2: :->subcommand' \
                '*::arg:->args' \
                && return 0

            case $state in
                command)
                    local -a commands
                    commands=(
                        'orm:Generate ORM projects to interact with a database.'
                        'lint:Analyse a database schema for potential issues.'
                        'report:Generate an HTML report of a database schema.'
                        'test:Test a database connection to see whether it is available.'
                        'completion:Generate shell completion scripts.'
                    )
                    _describe -t commands 'schematic command' commands
                    ;;
                subcommand)
                    case $line[1] in
                        orm)
                            local -a orm_commands
                            orm_commands=(
                                'efcore:Generate an Entity Framework Core project.'
                                'ormlite:Generate a ServiceStack OrmLite project.'
                                'poco:Generate a plain-old-CLR-object (POCO) project.'
                            )
                            _describe -t orm_commands 'orm command' orm_commands
                            ;;
                        completion)
                            _values 'shell' bash zsh fish powershell
                            ;;
                    esac
                    ;;
            esac
        }

        if [ "$funcstack[1]" = "_schematic" ]; then
            _schematic "$@"
        else
            compdef _schematic schematic
        fi
        """;

    private const string FishScript =
        """
        # fish completion for schematic
        #
        # Enable permanently:
        #   schematic completion fish > ~/.config/fish/completions/schematic.fish

        # Top-level commands
        complete -c schematic -f -n '__fish_use_subcommand' -a orm -d 'Generate ORM projects to interact with a database.'
        complete -c schematic -f -n '__fish_use_subcommand' -a lint -d 'Analyse a database schema for potential issues.'
        complete -c schematic -f -n '__fish_use_subcommand' -a report -d 'Generate an HTML report of a database schema.'
        complete -c schematic -f -n '__fish_use_subcommand' -a test -d 'Test a database connection to see whether it is available.'
        complete -c schematic -f -n '__fish_use_subcommand' -a completion -d 'Generate shell completion scripts.'

        # orm subcommands
        complete -c schematic -f -n '__fish_seen_subcommand_from orm; and not __fish_seen_subcommand_from efcore ormlite poco' -a efcore -d 'Generate an Entity Framework Core project.'
        complete -c schematic -f -n '__fish_seen_subcommand_from orm; and not __fish_seen_subcommand_from efcore ormlite poco' -a ormlite -d 'Generate a ServiceStack OrmLite project.'
        complete -c schematic -f -n '__fish_seen_subcommand_from orm; and not __fish_seen_subcommand_from efcore ormlite poco' -a poco -d 'Generate a plain-old-CLR-object (POCO) project.'

        # completion shells
        complete -c schematic -f -n '__fish_seen_subcommand_from completion' -a 'bash zsh fish powershell'

        # Options
        complete -c schematic -s c -l config -r -d 'Path to a configuration file.'
        complete -c schematic -l output -r -d 'The directory to save the generated report.'
        complete -c schematic -s t -l timeout -r -d 'A timeout (in seconds) to wait for.'
        complete -c schematic -l convention -r -d 'The naming convention to use.'
        complete -c schematic -l project-path -r -d 'The file path used to save the generated project.'
        complete -c schematic -l base-namespace -r -d 'A namespace that generated classes will belong in.'
        complete -c schematic -s h -l help -d 'Show help and usage information.'
        """;

    private const string PowerShellScript =
        """
        # PowerShell completion for schematic
        #
        # Enable for the current session:
        #   schematic completion powershell | Out-String | Invoke-Expression
        # Enable permanently, add the line above to your profile:
        #   schematic completion powershell >> $PROFILE

        using namespace System.Management.Automation
        using namespace System.Management.Automation.Language

        Register-ArgumentCompleter -Native -CommandName 'schematic' -ScriptBlock {
            param($wordToComplete, $commandAst, $cursorPosition)

            $commandElements = $commandAst.CommandElements
            $command = @(
                'schematic'
                for ($i = 1; $i -lt $commandElements.Count; $i++) {
                    $element = $commandElements[$i]
                    if ($element -isnot [StringConstantExpressionAst] -or
                        $element.StringConstantType -ne [StringConstantType]::BareWord -or
                        $element.Value.StartsWith('-')) {
                        break
                    }
                    $element.Value
                }
            ) -join ';'

            $completions = @(switch ($command) {
                'schematic' {
                    [CompletionResult]::new('orm', 'orm', [CompletionResultType]::ParameterValue, 'Generate ORM projects to interact with a database.')
                    [CompletionResult]::new('lint', 'lint', [CompletionResultType]::ParameterValue, 'Analyse a database schema for potential issues.')
                    [CompletionResult]::new('report', 'report', [CompletionResultType]::ParameterValue, 'Generate an HTML report of a database schema.')
                    [CompletionResult]::new('test', 'test', [CompletionResultType]::ParameterValue, 'Test a database connection to see whether it is available.')
                    [CompletionResult]::new('completion', 'completion', [CompletionResultType]::ParameterValue, 'Generate shell completion scripts.')
                    break
                }
                'schematic;orm' {
                    [CompletionResult]::new('efcore', 'efcore', [CompletionResultType]::ParameterValue, 'Generate an Entity Framework Core project.')
                    [CompletionResult]::new('ormlite', 'ormlite', [CompletionResultType]::ParameterValue, 'Generate a ServiceStack OrmLite project.')
                    [CompletionResult]::new('poco', 'poco', [CompletionResultType]::ParameterValue, 'Generate a plain-old-CLR-object (POCO) project.')
                    break
                }
                'schematic;completion' {
                    [CompletionResult]::new('bash', 'bash', [CompletionResultType]::ParameterValue, 'Bash')
                    [CompletionResult]::new('zsh', 'zsh', [CompletionResultType]::ParameterValue, 'Zsh')
                    [CompletionResult]::new('fish', 'fish', [CompletionResultType]::ParameterValue, 'Fish')
                    [CompletionResult]::new('powershell', 'powershell', [CompletionResultType]::ParameterValue, 'PowerShell')
                    break
                }
            })

            $completions.Where{ $_.CompletionText -like "$wordToComplete*" } |
                Sort-Object -Property ListItemText
        }
        """;
}
