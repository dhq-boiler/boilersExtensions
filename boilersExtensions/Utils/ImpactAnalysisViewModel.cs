﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;

namespace boilersExtensions.Utils
{
    /// <summary>
    /// 型置換の影響範囲分析結果を表示するためのViewModel
    /// </summary>
    public class ImpactAnalysisViewModel : BindableBase, IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();

        public ImpactAnalysisViewModel()
        {
            // ダイアログを閉じるコマンド
            CloseCommand = new ReactiveCommand()
                .AddTo(_compositeDisposable);

            CloseCommand.Subscribe(() =>
            {
                Window?.Close();
            })
            .AddTo(_compositeDisposable);

            // 参照箇所に移動するコマンド
            NavigateToReferenceCommand = new ReactiveCommand<TypeReferenceInfo>()
                .AddTo(_compositeDisposable);

            NavigateToReferenceCommand.Subscribe(reference =>
                {
                    if (reference != null)
                    {
                        NavigateToReference(reference);
                    }
                })
                .AddTo(_compositeDisposable);

            // ブックマークトグルコマンド
            ToggleBookmarkCommand = new ReactiveCommand<TypeReferenceInfo>()
                .AddTo(_compositeDisposable);

            ToggleBookmarkCommand.Subscribe(async reference =>
                {
                    if (reference != null)
                    {
                        // BookmarkManagerを使用してブックマークをトグル
                        await BookmarkManager.ToggleBookmarkAsync(reference.FilePath, reference.LineNumber);
                    }
                })
                .AddTo(_compositeDisposable);
        }

        // コマンド
        public ReactiveCommand CloseCommand { get; }
        public ReactiveCommand<TypeReferenceInfo> NavigateToReferenceCommand { get; }
        public ReactiveCommand<TypeReferenceInfo> ToggleBookmarkCommand { get; }

        // 影響分析の基本情報
        public string OriginalTypeName { get; set; }
        public string NewTypeName { get; set; }
        public int ReferencesCount { get; set; }

        // 参照箇所のリスト
        public List<TypeReferenceInfo> References { get; set; } = new List<TypeReferenceInfo>();

        // 潜在的な問題のリスト
        public List<PotentialIssue> PotentialIssues { get; set; } = new List<PotentialIssue>();

        // ダイアログへの参照
        public Window Window { get; set; }

        // 参照箇所に移動するメソッド
        private void NavigateToReference(TypeReferenceInfo reference)
        {
            // UIスレッドで実行
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                try
                {
                    // DTEサービスを取得
                    var dte = (EnvDTE.DTE)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(EnvDTE.DTE));
                    if (dte == null)
                        return;

                    // ファイルを開く
                    var window = dte.ItemOperations.OpenFile(reference.FilePath);
                    if (window != null)
                    {
                        // TextDocumentを取得
                        var textDoc = window.Document.Object("TextDocument") as EnvDTE.TextDocument;
                        if (textDoc != null)
                        {
                            // 指定した行にカーソルを移動
                            var point = textDoc.StartPoint.CreateEditPoint();
                            point.MoveToLineAndOffset(reference.LineNumber, reference.Column);

                            // 選択状態にする
                            var line = textDoc.StartPoint.CreateEditPoint();
                            line.MoveToLineAndOffset(reference.LineNumber, 1);
                            var lineEnd = line.CreateEditPoint();
                            lineEnd.EndOfLine();

                            // 行全体を選択
                            textDoc.Selection.MoveToPoint(line);
                            textDoc.Selection.MoveToPoint(lineEnd, true);

                            // エディタにフォーカスを設定
                            window.Activate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Navigation error: {ex.Message}");
                    MessageBox.Show($"参照箇所への移動中にエラーが発生しました: {ex.Message}",
                        "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        // ダイアログが開かれた時にブックマーク状態を初期化
        public async void OnDialogOpened(Window window)
        {
            Window = window;

            // 各参照のブックマーク状態を初期化
            await InitializeBookmarkStatesAsync();
        }

        // 参照のブックマーク状態を初期化するメソッド
        private async Task InitializeBookmarkStatesAsync()
        {
            foreach (var reference in References)
            {
                try
                {
                    // 現在のブックマーク状態を確認（ファイルと行番号を指定）
                    bool isBookmarked = BookmarkManager.IsBookmarkSet(reference.FilePath, reference.LineNumber);
                    reference.IsBookmarked.Value = isBookmarked;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error initializing bookmark state: {ex.Message}");
                    // エラー時はデフォルトでfalse
                    reference.IsBookmarked.Value = false;
                }
            }
        }

        public void Dispose()
        {
            _compositeDisposable?.Dispose();
            CloseCommand?.Dispose();
        }
    }

    /// <summary>
    /// 型参照情報を格納するクラス
    /// </summary>
    public class TypeReferenceInfo
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public int Column { get; set; }
        public string Text { get; set; }
        public string ReferenceType { get; set; } // Method parameter, variable declaration, etc.

        // ブックマーク状態を保持するプロパティを追加
        public ReactivePropertySlim<bool> IsBookmarked { get; } = new ReactivePropertySlim<bool>(false);
    }

    /// <summary>
    /// 型置換による潜在的な問題を表すクラス
    /// </summary>
    public class PotentialIssue
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public string IssueType { get; set; } // MethodMissing, PropertyMissing など
        public string Description { get; set; }
        public string SuggestedFix { get; set; }
        public string CodeSnippet { get; set; }
    }
}
